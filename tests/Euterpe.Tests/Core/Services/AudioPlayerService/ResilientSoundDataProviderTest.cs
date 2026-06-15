using SoundFlow.Enums;
using SoundFlow.Interfaces;
using SoundFlow.Metadata.Models;

namespace Euterpe.Tests.Core;

[Category("ResilientSoundDataProviderTests")]
[TestSubject(typeof(ResilientSoundDataProvider))]
public sealed class ResilientSoundDataProviderTest
{
    private static ResilientSoundDataProvider CreateProvider(FakeSoundDataProvider inner) =>
        new(inner, Mock.Logger<AudioPlayerService>());

    [Test]
    public async Task ReadBytes_InnerSucceeds_PassesThrough()
    {
        var inner = new FakeSoundDataProvider { SamplesPerRead = 16 };
        var provider = CreateProvider(inner);

        var read = provider.ReadBytes(new float[32]);

        await Assert.That(read).IsEqualTo(16);
    }

    [Test]
    public async Task ReadBytes_InnerThrows_ReturnsZeroAndStopsRetrying()
    {
        var inner = new FakeSoundDataProvider { ShouldThrow = true };
        var provider = CreateProvider(inner);
        var buffer = new float[16];

        var first = provider.ReadBytes(buffer);
        var second = provider.ReadBytes(buffer);

        using var _ = Assert.Multiple();
        await Assert.That(first).IsEqualTo(0);
        await Assert.That(second).IsEqualTo(0);
        await Assert.That(inner.ReadCalls).IsEqualTo(1);
    }

    private sealed class FakeSoundDataProvider : ISoundDataProvider
    {
        public bool ShouldThrow { get; init; }
        public int SamplesPerRead { get; init; }
        public int ReadCalls { get; private set; }

        public int Position => 0;
        public int Length => 100;
        public bool CanSeek => false;
        public SampleFormat SampleFormat => SampleFormat.F32;
        public int SampleRate => 44100;
        public bool IsDisposed { get; private set; }
        public SoundFormatInfo? FormatInfo => null;

        public event EventHandler<EventArgs>? EndOfStreamReached
        {
            add { }
            remove { }
        }

        public event EventHandler<PositionChangedEventArgs>? PositionChanged
        {
            add { }
            remove { }
        }

        public int ReadBytes(Span<float> buffer)
        {
            ReadCalls++;
            return ShouldThrow ? throw new InvalidDataException() : SamplesPerRead;
        }

        public void Seek(int offset)
        {
        }

        public void Dispose() => IsDisposed = true;
    }
}
