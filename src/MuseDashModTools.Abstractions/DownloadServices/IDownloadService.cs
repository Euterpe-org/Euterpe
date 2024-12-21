using Downloader;

namespace MuseDashModTools.Abstractions;

public interface IDownloadService
{
    Task<bool> DownloadMelonLoaderAsync(
        EventHandler<DownloadStartedEventArgs> onDownloadStarted,
        IProgress<double> downloadProgress,
        CancellationToken cancellationToken = default);

    Task<bool> DownloadModAsync(ModDto mod, CancellationToken cancellationToken = default);
    Task<bool> DownloadLibAsync(string libName, CancellationToken cancellationToken = default);
    Task<string?> FetchReadmeAsync(string repoId, CancellationToken cancellationToken = default);
    IAsyncEnumerable<Mod?> GetModListAsync(CancellationToken cancellationToken = default);
}