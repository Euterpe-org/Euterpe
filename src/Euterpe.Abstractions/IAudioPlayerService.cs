namespace Euterpe.Abstractions;

public interface IAudioPlayerService : IDisposable
{
    /// <summary>Raised when the current playback reaches its end (or stops itself on a decode failure).</summary>
    event EventHandler? PlaybackEnded;

    void Play(string filePath);
    void Pause();
    void Resume();
    void Stop();
}
