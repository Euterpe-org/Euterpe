namespace MuseDashModTools.Abstractions;

public interface IDownloadService
{
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
    Task DownloadReleaseByTagAsync(string tag, string osString, string updateFolder, CancellationToken cancellationToken = default);
    Task<string?> FetchReadmeAsync(string repoId, CancellationToken cancellationToken = default);
    IAsyncEnumerable<Mod?> GetModListAsync(CancellationToken cancellationToken = default);
    IAsyncEnumerable<Lib?> GetLibListAsync(CancellationToken cancellationToken = default);
}