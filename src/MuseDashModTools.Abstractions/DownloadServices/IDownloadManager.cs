namespace MuseDashModTools.Abstractions;

public interface IDownloadManager : IDownloadService
{
    /// <summary>
    ///     Download file from URL with progress reporting
    /// </summary>
    /// <param name="url">Download URL</param>
    /// <param name="filePath">Local file path to save</param>
    /// <param name="onDownloadStarted">Download started event handler</param>
    /// <param name="downloadProgress">Progress reporter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if download successful, false otherwise</returns>
    Task<bool> DownloadFileAsync(
        string url,
        string filePath,
        EventHandler<DownloadStartedEventArgs>? onDownloadStarted = null,
        IProgress<double>? downloadProgress = null,
        CancellationToken cancellationToken = default);
}