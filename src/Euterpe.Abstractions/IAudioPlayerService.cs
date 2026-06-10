namespace Euterpe.Abstractions;

public interface IAudioPlayerService : IDisposable
{
    string? PlayingFilePath { get; }

    event EventHandler<string?>? PlayingFileChanged;

    void Play(string filePath);
    void Pause();
    void Resume();
    void Stop();
}
