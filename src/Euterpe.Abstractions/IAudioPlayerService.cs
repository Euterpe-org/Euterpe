namespace Euterpe.Abstractions;

public interface IAudioPlayerService : IDisposable
{
    event EventHandler? PlaybackEnded;

    void Play(string filePath);
    void Pause();
    void Resume();
    void Stop();
}
