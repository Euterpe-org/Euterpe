using NVorbis;
using SoundFlow.Enums;
using SoundFlow.Interfaces;

namespace Euterpe.Core.Audio.Codecs;

internal sealed class VorbisDecoder : ISoundDecoder
{
    private readonly bool _canSeek;
    private readonly VorbisReader _reader;
    private bool _endOfStreamReached;

    public VorbisDecoder(Stream stream)
    {
        _canSeek = stream.CanSeek;
        _reader = new VorbisReader(stream, false);
        Length = (int)(_reader.TotalSamples * _reader.Channels);
    }

    public bool IsDisposed { get; private set; }
    public int Length { get; }
    public SampleFormat SampleFormat => SampleFormat.F32;
    public int Channels => _reader.Channels;
    public int SampleRate => _reader.SampleRate;

    public event EventHandler<EventArgs>? EndOfStreamReached;

    public int Decode(Span<float> samples)
    {
        if (IsDisposed || _endOfStreamReached || samples.Length < Channels)
        {
            return 0;
        }

        var samplesRead = _reader.ReadSamples(samples);
        if (samplesRead is 0)
        {
            _endOfStreamReached = true;
            EndOfStreamReached?.Invoke(this, EventArgs.Empty);
        }

        return samplesRead;
    }

    public bool Seek(int sampleOffset)
    {
        if (IsDisposed || !_canSeek)
        {
            return false;
        }

        _reader.SeekTo(sampleOffset / Channels);
        _endOfStreamReached = false;
        return true;
    }

    public void Dispose()
    {
        if (IsDisposed)
        {
            return;
        }

        _reader.Dispose();
        IsDisposed = true;
    }
}
