using SoundFlow.Enums;
using SoundFlow.Interfaces;
using SoundFlow.Metadata.Models;

namespace Euterpe.Core;

/// <summary>Turns a mid-stream decode failure (e.g. corrupt audio) into a clean end-of-stream instead of an unhandled exception on the audio thread.</summary>
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

    public int ReadBytes(Span<float> buffer)
    {
        try
        {
            return inner.ReadBytes(buffer);
        }
        catch (Exception ex)
        {
            logger.ZLogWarning(ex, $"Audio decode failed; stopping preview playback");
            return 0;
        }
    }

    public void Seek(int offset) => inner.Seek(offset);

    public void Dispose() => inner.Dispose();
}
