namespace Euterpe.Abstractions;

public interface IDependencyAcquireService
{
    Task AcquireForMelonLoaderAsync(
        EventHandler<DownloadStartedEventArgs>? onDownloadStarted = null,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default);

    Task<string?> GetLatestMelonLoaderVersionAsync(CancellationToken cancellationToken = default);
}