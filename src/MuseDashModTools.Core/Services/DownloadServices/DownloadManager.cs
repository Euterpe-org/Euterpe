namespace MuseDashModTools.Core;

[DownloadManager]
internal sealed partial class DownloadManager : IDownloadManager
{
    public async Task<bool> DownloadFileAsync(
        string url,
        string filePath,
        EventHandler<DownloadStartedEventArgs>? onDownloadStarted = null,
        IProgress<double>? downloadProgress = null,
        CancellationToken cancellationToken = default)
    {
        EventHandler<DownloadProgressChangedEventArgs>? progressHandler = null;

        if (onDownloadStarted is not null)
        {
            Downloader.DownloadStarted += onDownloadStarted;
        }

        if (downloadProgress is not null)
        {
            progressHandler = (_, e) => downloadProgress.Report(e.ProgressPercentage);
            Downloader.DownloadProgressChanged += progressHandler;
        }

        try
        {
            await Downloader.DownloadFileTaskAsync(url, filePath, cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to download file from {url} to {filePath}");
            return false;
        }
        finally
        {
            if (onDownloadStarted is not null)
            {
                Downloader.DownloadStarted -= onDownloadStarted;
            }

            if (progressHandler is not null)
            {
                Downloader.DownloadProgressChanged -= progressHandler;
            }
        }
    }

    public Task<bool> DownloadMelonLoaderAsync(
        EventHandler<DownloadStartedEventArgs> onDownloadStarted,
        IProgress<double> downloadProgress,
        CancellationToken cancellationToken = default)
    {
        return Config.DownloadSource switch
        {
            DownloadSource.GitHub => GitHubDownloadService.DownloadMelonLoaderAsync(onDownloadStarted, downloadProgress, cancellationToken),
            DownloadSource.GitHubMirror => GitHubMirrorDownloadService.DownloadMelonLoaderAsync(onDownloadStarted, downloadProgress, cancellationToken),
            // For Gitee Download Source, because users don't choose GitHub for other downloads, so we will use GitHubMirror
            DownloadSource.Gitee => GitHubMirrorDownloadService.DownloadMelonLoaderAsync(onDownloadStarted, downloadProgress, cancellationToken),
            DownloadSource.Website => WebsiteDownloadService.DownloadMelonLoaderAsync(onDownloadStarted, downloadProgress, cancellationToken),
            _ => throw new UnreachableException()
        };
    }

    public Task<string?> FetchReadmeAsync(string repoId, CancellationToken cancellationToken = default)
    {
        return Config.DownloadSource switch
        {
            DownloadSource.GitHub => GitHubDownloadService.FetchReadmeAsync(repoId, cancellationToken),
            DownloadSource.GitHubMirror => GitHubMirrorDownloadService.FetchReadmeAsync(repoId, cancellationToken),
            // For Gitee Download Source, because users don't choose GitHub for other downloads, so we will use GitHubMirror
            DownloadSource.Gitee => GitHubMirrorDownloadService.FetchReadmeAsync(repoId, cancellationToken),
            DownloadSource.Website => WebsiteDownloadService.FetchReadmeAsync(repoId, cancellationToken),
            _ => throw new UnreachableException()
        };
    }

    #region Injections

    [UsedImplicitly]
    public required Config Config { get; init; }

    [UsedImplicitly]
    public required MultiThreadDownloader Downloader { get; init; }

    [UsedImplicitly]
    public required IWebsiteDownloadService WebsiteDownloadService { get; init; }

    [UsedImplicitly]
    public required IGiteeDownloadService GiteeDownloadService { get; init; }

    [UsedImplicitly]
    public required IGitHubDownloadService GitHubDownloadService { get; init; }

    [UsedImplicitly]
    public required IGitHubMirrorDownloadService GitHubMirrorDownloadService { get; init; }

    [UsedImplicitly]
    public required ILogger<DownloadManager> Logger { get; init; }

    #endregion Injections
}