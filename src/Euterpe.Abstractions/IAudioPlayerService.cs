namespace Euterpe.Abstractions;

public interface IAudioPlayerService : IDisposable
{
    void Play(string filePath);
    void Pause();
    void Resume();
    void Stop();
}
