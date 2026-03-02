namespace Euterpe.Abstractions;

public interface IDownloadManager
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

    Task<bool> DownloadMelonLoaderAsync(
        EventHandler<DownloadStartedEventArgs> onDownloadStarted,
        EventHandler<DownloadProgressChangedEventArgs> onDownloadProgressChanged,
        CancellationToken cancellationToken = default);

    Task<bool> DownloadUnityDependencyAsync(
        EventHandler<DownloadStartedEventArgs> onDownloadStarted,
        EventHandler<DownloadProgressChangedEventArgs> onDownloadProgressChanged,
        CancellationToken cancellationToken = default);

    Task<bool> DownloadCpp2ILExecutableAsync(
        EventHandler<DownloadStartedEventArgs> onDownloadStarted,
        EventHandler<DownloadProgressChangedEventArgs> onDownloadProgressChanged,
        CancellationToken cancellationToken = default);

    Task<bool> DownloadCpp2ILPluginAsync(
        EventHandler<DownloadStartedEventArgs> onDownloadStarted,
        EventHandler<DownloadProgressChangedEventArgs> onDownloadProgressChanged,
        CancellationToken cancellationToken = default);

    Task<bool> DownloadModAsync(ModDto mod, CancellationToken cancellationToken = default);
    Task<bool> DownloadLibAsync(LibDto lib, CancellationToken cancellationToken = default);
    Task DownloadReleaseByTagAsync(string tag, string runtimeIdentifier, string updateFolder, CancellationToken cancellationToken = default);
    Task<string?> FetchReadmeAsync(string repoId, CancellationToken cancellationToken = default);
    IAsyncEnumerable<Mod?> GetModListAsync(CancellationToken cancellationToken = default);
    IAsyncEnumerable<Lib?> GetLibListAsync(CancellationToken cancellationToken = default);
}