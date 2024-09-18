namespace MuseDashModToolsUI.Contracts;

public interface IDownloadService
{
    Task CheckForUpdatesAsync(CancellationToken cancellationToken = default);

    Task<bool> DownloadMelonLoaderAsync(
        EventHandler<DownloadStartedEventArgs> onDownloadStarted,
        IProgress<double> downloadProgress,
        CancellationToken cancellationToken = default);

    Task<bool> DownloadModAsync(ModDto mod, CancellationToken cancellationToken = default);
    Task<bool> DownloadLibAsync(string libFileName, CancellationToken cancellationToken = default);
    Task<Mod[]?> GetModListAsync(CancellationToken cancellationToken = default);
}