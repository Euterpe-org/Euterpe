namespace Euterpe.Abstractions;

public interface IAudioPlayerService : IDisposable
{
    Task PlayAsync(string key, string filePath);
    void Pause();
    void Resume();
    void Stop();
}
