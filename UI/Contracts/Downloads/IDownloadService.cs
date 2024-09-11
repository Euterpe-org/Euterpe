namespace MuseDashModToolsUI.Contracts.Downloads;

public interface IDownloadService
{
    Task<Mod[]?> GetModListAsync(CancellationToken cancellationToken = default);

    Task<bool> DownloadMelonLoaderAsync(
        EventHandler<DownloadStartedEventArgs> onDownloadStarted,
        IProgress<double> downloadProgress,
        CancellationToken cancellationToken = default);
}