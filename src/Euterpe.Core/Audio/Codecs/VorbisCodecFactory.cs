using SoundFlow.Interfaces;
using SoundFlow.Structs;

namespace Euterpe.Core.Audio.Codecs;

internal sealed class VorbisCodecFactory : ICodecFactory
{
    public string FactoryId => "Euterpe.NVorbis";
    public IReadOnlyCollection<string> SupportedFormatIds { get; } = ["ogg"];
    public int Priority => 100;

    public ISoundDecoder? CreateDecoder(Stream stream, string formatId, AudioFormat format) =>
        SupportedFormatIds.Contains(formatId) ? new VorbisDecoder(stream) : null;

    public ISoundDecoder TryCreateDecoder(Stream stream, out AudioFormat detectedFormat, AudioFormat? hintFormat = null)
    {
        var decoder = new VorbisDecoder(stream);
        detectedFormat = new AudioFormat
        {
            Format = decoder.SampleFormat,
            Channels = decoder.Channels,
            Layout = AudioFormat.GetLayoutFromChannels(decoder.Channels),
            SampleRate = decoder.SampleRate
        };

        return decoder;
    }

    public ISoundEncoder? CreateEncoder(Stream stream, string formatId, AudioFormat format) => null;
}
