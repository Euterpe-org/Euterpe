namespace Euterpe.Abstractions;

public interface IDependencyAcquireService
{
    Task EnsureValidAsync(
        DependencySpec spec,
        EventHandler<DownloadStartedEventArgs>? onDownloadStarted = null,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default);
}