namespace Euterpe.Abstractions;

public interface IUpdateService
{
    Task<bool> CheckForUpdatesAsync(CancellationToken cancellationToken = default);
}
