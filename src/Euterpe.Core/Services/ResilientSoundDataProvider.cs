using SoundFlow.Enums;
using SoundFlow.Interfaces;
using SoundFlow.Metadata.Models;

namespace Euterpe.Core;

internal sealed class ResilientSoundDataProvider(ISoundDataProvider inner, ILogger logger) : ISoundDataProvider
{
    public int Position => inner.Position;
    public int Length => inner.Length;
    public bool CanSeek => inner.CanSeek;
    public SampleFormat SampleFormat => inner.SampleFormat;
    public int SampleRate => inner.SampleRate;
    public bool IsDisposed => inner.IsDisposed;
    public SoundFormatInfo? FormatInfo => inner.FormatInfo;

    public event EventHandler<EventArgs>? EndOfStreamReached
    {
        add => inner.EndOfStreamReached += value;
        remove => inner.EndOfStreamReached -= value;
    }

    public event EventHandler<PositionChangedEventArgs>? PositionChanged
    {
        add => inner.PositionChanged += value;
        remove => inner.PositionChanged -= value;
    }

    private bool _failed;

    public int ReadBytes(Span<float> buffer)
    {
        if (_failed)
        {
            return 0;
        }

        try
        {
            return inner.ReadBytes(buffer);
        }
        catch (Exception ex)
        {
            _failed = true;
            logger.ZLogWarning(ex, $"Audio decode failed; stopping preview playback");
            return 0;
        }
    }

    public void Seek(int offset) => inner.Seek(offset);

    public void Dispose() => inner.Dispose();
}
