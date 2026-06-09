using SoundFlow.Enums;
using SoundFlow.Interfaces;
using SoundFlow.Metadata.Models;

namespace Euterpe.Core;

/// <summary>
/// Wraps a sound data provider so a mid-stream decode failure (e.g. a corrupt audio file) is
/// caught on the audio callback thread and turned into a graceful end-of-stream, instead of an
/// unhandled exception that tears down the whole process.
/// </summary>
internal sealed class ResilientSoundDataProvider(ISoundDataProvider inner, ILogger logger) : ISoundDataProvider
{
    public int Position => inner.Position;
    public int Length => inner.Length;
    public bool CanSeek => inner.CanSeek;
    public SampleFormat SampleFormat => inner.SampleFormat;
    public int SampleRate => inner.SampleRate;
    public bool IsDisposed => inner.IsDisposed;
    public SoundFormatInfo FormatInfo => inner.FormatInfo;

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
            // Corrupt / undecodable audio: end the stream cleanly (0 = no more samples) instead of
            // letting the exception escape the audio thread and crash the process.
            logger.ZLogWarning(ex, $"Audio decode failed; stopping preview playback");
            return 0;
        }
    }

    public void Seek(int offset) => inner.Seek(offset);

    public void Dispose() => inner.Dispose();
}
