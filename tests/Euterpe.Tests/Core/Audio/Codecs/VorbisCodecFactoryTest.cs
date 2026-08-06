using Euterpe.Core.Audio.Codecs;
using SoundFlow.Backends.MiniAudio;
using SoundFlow.Enums;
using SoundFlow.Providers;

namespace Euterpe.Tests.Core.Audio.Codecs;

[Category("VorbisCodecFactoryTests")]
[TestSubject(typeof(VorbisCodecFactory))]
public sealed class VorbisCodecFactoryTest
{
    private static string FixturePath => Path.Combine(AppContext.BaseDirectory, "Fixtures", "Tone.ogg");

    [Test]
    public async Task CreateDecoder_VorbisStream_DecodesFloatSamples()
    {
        using var stream = File.OpenRead(FixturePath);
        using var decoder = new VorbisCodecFactory().CreateDecoder(stream, "ogg", default);

        await Assert.That(decoder).IsNotNull();

        var samples = new float[2048];
        var samplesRead = decoder!.Decode(samples);
        var containsAudibleSamples = samples.AsSpan(0, samplesRead).ContainsAnyExcept(0f);
        var totalSamplesRead = samplesRead;
        int nextSamplesRead;
        while ((nextSamplesRead = decoder.Decode(samples)) > 0)
        {
            totalSamplesRead += nextSamplesRead;
        }

        using var _ = Assert.Multiple();
        await Assert.That(decoder.SampleFormat).IsEqualTo(SampleFormat.F32);
        await Assert.That(decoder.Channels).IsEqualTo(2);
        await Assert.That(decoder.SampleRate).IsEqualTo(44100);
        await Assert.That(decoder.Length).IsEqualTo(totalSamplesRead);
        await Assert.That(samplesRead).IsGreaterThan(0);
        await Assert.That(samplesRead % decoder.Channels).IsEqualTo(0);
        await Assert.That(containsAudibleSamples).IsTrue();
    }

    [Test]
    public async Task Seek_AfterDecode_RepeatsSamplesFromRequestedPosition()
    {
        using var stream = File.OpenRead(FixturePath);
        using var decoder = new VorbisCodecFactory().CreateDecoder(stream, "ogg", default)!;
        var firstRead = new float[512];
        var secondRead = new float[512];

        var firstCount = decoder.Decode(firstRead);
        var seekSucceeded = decoder.Seek(0);
        var secondCount = decoder.Decode(secondRead);

        using var _ = Assert.Multiple();
        await Assert.That(seekSucceeded).IsTrue();
        await Assert.That(secondCount).IsEqualTo(firstCount);
        await Assert.That(secondRead.AsSpan(0, secondCount).SequenceEqual(firstRead.AsSpan(0, firstCount))).IsTrue();
    }

    [Test]
    public async Task Decode_EndOfStreamReached_RaisesOnce()
    {
        using var stream = File.OpenRead(FixturePath);
        using var decoder = new VorbisCodecFactory().CreateDecoder(stream, "ogg", default)!;
        var eventCount = 0;
        decoder.EndOfStreamReached += (_, _) => eventCount++;
        var samples = new float[1024];

        while (decoder.Decode(samples) > 0)
        {
        }

        decoder.Decode(samples);

        await Assert.That(eventCount).IsEqualTo(1);
    }

    [Test]
    public async Task Dispose_Decoder_DoesNotDisposeInputStream()
    {
        using var stream = File.OpenRead(FixturePath);
        var decoder = new VorbisCodecFactory().CreateDecoder(stream, "ogg", default)!;

        decoder.Dispose();

        await Assert.That(stream.CanRead).IsTrue();
    }

    [Test]
    public async Task StreamDataProvider_RegisteredFactory_DecodesVorbisStream()
    {
        using var engine = new MiniAudioEngine();
        engine.RegisterCodecFactory(new VorbisCodecFactory());
        using var stream = File.OpenRead(FixturePath);
        using var provider = new StreamDataProvider(engine, stream);

        var samplesRead = provider.ReadBytes(new float[1024]);

        using var _ = Assert.Multiple();
        await Assert.That(provider.FormatInfo!.FormatIdentifier).IsEqualTo("ogg");
        await Assert.That(provider.SampleFormat).IsEqualTo(SampleFormat.F32);
        await Assert.That(provider.SampleRate).IsEqualTo(44100);
        await Assert.That(samplesRead).IsGreaterThan(0);
    }
}
