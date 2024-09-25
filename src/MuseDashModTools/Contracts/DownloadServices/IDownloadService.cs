namespace MuseDashModTools.Contracts;

public interface IDownloadService
{
    Task CheckForUpdatesAsync(CancellationToken cancellationToken = default);

    Task<bool> DownloadMelonLoaderAsync(
        EventHandler<DownloadStartedEventArgs> onDownloadStarted,
        IProgress<double> downloadProgress,
        CancellationToken cancellationToken = default);

    Task<bool> DownloadModAsync(ModDto mod, CancellationToken cancellationToken = default);
    Task<bool> DownloadLibAsync(string libName, CancellationToken cancellationToken = default);
    IAsyncEnumerable<Mod?> GetModListAsync(CancellationToken cancellationToken = default);
}