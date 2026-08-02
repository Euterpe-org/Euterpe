namespace Euterpe.Abstractions;

public interface IDependencyAcquireService
{
    Task AcquireForMelonLoaderAsync(
        EventHandler<DownloadStartedEventArgs>? onDownloadStarted = null,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default);

    Task<MelonLoaderRelease> GetLatestMelonLoaderReleaseAsync(CancellationToken cancellationToken = default);
}
