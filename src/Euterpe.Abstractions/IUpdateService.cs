namespace Euterpe.Abstractions;

public interface IUpdateService
{
    Task ConfigureDownloadSourceAsync(CancellationToken cancellationToken = default);
    Task<bool> CheckForUpdatesAsync(CancellationToken cancellationToken = default);
}