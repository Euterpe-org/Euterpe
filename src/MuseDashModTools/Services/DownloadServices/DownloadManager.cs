namespace MuseDashModTools.Services;

public sealed class DownloadManager : IDownloadManager
{
    public Task CheckForUpdatesAsync(CancellationToken cancellationToken = default)
    {
        return Setting.DownloadSource switch
        {
            DownloadSource.GitHub => GitHubDownloadService.CheckForUpdatesAsync(cancellationToken),
            DownloadSource.GitHubMirror => GitHubMirrorDownloadService.CheckForUpdatesAsync(cancellationToken),
            // For Custom Download Source, because they don't choose GitHub for other downloads, so we will check updates from GitHubMirror
            DownloadSource.Custom => GitHubMirrorDownloadService.CheckForUpdatesAsync(cancellationToken),
            _ => throw new UnreachableException()
        };
    }

    public Task<bool> DownloadMelonLoaderAsync(
        EventHandler<DownloadStartedEventArgs> onDownloadStarted,
        IProgress<double> downloadProgress,
        CancellationToken cancellationToken = default)
    {
        return Setting.DownloadSource switch
        {
            DownloadSource.GitHub => GitHubDownloadService.DownloadMelonLoaderAsync(onDownloadStarted, downloadProgress, cancellationToken),
            DownloadSource.GitHubMirror => GitHubMirrorDownloadService.DownloadMelonLoaderAsync(onDownloadStarted, downloadProgress, cancellationToken),
            DownloadSource.Custom => CustomDownloadService.DownloadMelonLoaderAsync(onDownloadStarted, downloadProgress, cancellationToken),
            _ => throw new UnreachableException()
        };
    }

    public Task<bool> DownloadModAsync(ModDto mod, CancellationToken cancellationToken = default)
    {
        return Setting.DownloadSource switch
        {
            DownloadSource.GitHub => GitHubDownloadService.DownloadModAsync(mod, cancellationToken),
            DownloadSource.GitHubMirror => GitHubMirrorDownloadService.DownloadModAsync(mod, cancellationToken),
            DownloadSource.Custom => CustomDownloadService.DownloadModAsync(mod, cancellationToken),
            _ => throw new UnreachableException()
        };
    }

    public Task<bool> DownloadLibAsync(string libName, CancellationToken cancellationToken = default)
    {
        return Setting.DownloadSource switch
        {
            DownloadSource.GitHub => GitHubDownloadService.DownloadLibAsync(libName, cancellationToken),
            DownloadSource.GitHubMirror => GitHubMirrorDownloadService.DownloadLibAsync(libName, cancellationToken),
            DownloadSource.Custom => CustomDownloadService.DownloadLibAsync(libName, cancellationToken),
            _ => throw new UnreachableException()
        };
    }

    public IAsyncEnumerable<Mod?> GetModListAsync(CancellationToken cancellationToken = default)
    {
        return Setting.DownloadSource switch
        {
            DownloadSource.GitHub => GitHubDownloadService.GetModListAsync(cancellationToken),
            DownloadSource.GitHubMirror => GitHubMirrorDownloadService.GetModListAsync(cancellationToken),
            DownloadSource.Custom => CustomDownloadService.GetModListAsync(cancellationToken),
            _ => throw new UnreachableException()
        };
    }

    #region Injections

    [UsedImplicitly]
    public ICustomDownloadService CustomDownloadService { get; init; } = null!;

    [UsedImplicitly]
    public IGitHubDownloadService GitHubDownloadService { get; init; } = null!;

    [UsedImplicitly]
    public IGitHubMirrorDownloadService GitHubMirrorDownloadService { get; init; } = null!;

    [UsedImplicitly]
    public Setting Setting { get; init; } = null!;

    #endregion Injections
}