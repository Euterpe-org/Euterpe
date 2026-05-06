namespace Euterpe.Abstractions;

public interface IAppDownloadManager
{
    /// <summary>
    ///     Download file from URL with progress reporting. Throws on failure.
    /// </summary>
    /// <param name="url">Download URL</param>
    /// <param name="filePath">Local file path to save</param>
    /// <param name="onDownloadStarted">Download started event handler</param>
    /// <param name="downloadProgress">Progress reporter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DownloadFileAsync(
        string url,
        string filePath,
        EventHandler<DownloadStartedEventArgs>? onDownloadStarted = null,
        IProgress<double>? downloadProgress = null,
        CancellationToken cancellationToken = default);

    Task DownloadAssetAsync(string downloadUrl, string filePath, string displayName, CancellationToken cancellationToken = default);
    Task DownloadReleaseAsync(string downloadUrl, string updateFolder, CancellationToken cancellationToken = default);
    Task<string?> FetchReadmeAsync(string repoId, CancellationToken cancellationToken = default);
}