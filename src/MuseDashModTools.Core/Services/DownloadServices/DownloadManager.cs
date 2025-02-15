namespace MuseDashModTools.Core;

internal sealed class DownloadManager : IDownloadManager
{
    private IDownloadService CurrentDownloadService => Config.DownloadSource switch
    {
        DownloadSource.GitHub => GitHubDownloadService,
        DownloadSource.GitHubMirror => GitHubMirrorDownloadService,
        DownloadSource.Custom => CustomDownloadService,
        _ => throw new UnreachableException()
    };

    public Task<bool> DownloadMelonLoaderAsync(EventHandler<DownloadStartedEventArgs> onDownloadStarted, IProgress<double> downloadProgress,
        CancellationToken cancellationToken = default) =>
        CurrentDownloadService.DownloadMelonLoaderAsync(onDownloadStarted, downloadProgress, cancellationToken);

    public Task<bool> DownloadModAsync(ModDto mod, CancellationToken cancellationToken = default) =>
        CurrentDownloadService.DownloadModAsync(mod, cancellationToken);

    public Task<bool> DownloadLibAsync(string libName, CancellationToken cancellationToken = default) =>
        CurrentDownloadService.DownloadLibAsync(libName, cancellationToken);

    public Task DownloadReleaseByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        return Config.DownloadSource switch
        {
            DownloadSource.GitHub => GitHubDownloadService.DownloadReleaseByTagAsync(tag, cancellationToken),
            DownloadSource.GitHubMirror => GitHubMirrorDownloadService.DownloadReleaseByTagAsync(tag, cancellationToken),
            // For Custom Download Source, because they don't choose GitHub for other downloads, so we will use GitHubMirror
            DownloadSource.Custom => GitHubMirrorDownloadService.DownloadReleaseByTagAsync(tag, cancellationToken),
            _ => throw new UnreachableException()
        };
    }

    public Task<string?> FetchReadmeAsync(string repoId, CancellationToken cancellationToken = default)
    {
        return Config.DownloadSource switch
        {
            DownloadSource.GitHub => GitHubDownloadService.FetchReadmeAsync(repoId, cancellationToken),
            DownloadSource.GitHubMirror => GitHubMirrorDownloadService.FetchReadmeAsync(repoId, cancellationToken),
            // For Custom Download Source, because they don't choose GitHub for other downloads, so we will use GitHubMirror
            DownloadSource.Custom => GitHubMirrorDownloadService.FetchReadmeAsync(repoId, cancellationToken),
            _ => throw new UnreachableException()
        };
    }

    public IAsyncEnumerable<Mod?> GetModListAsync(CancellationToken cancellationToken = default) =>
        CurrentDownloadService.GetModListAsync(cancellationToken);

    #region Injections

    [UsedImplicitly]
    public ICustomDownloadService CustomDownloadService { get; init; } = null!;

    [UsedImplicitly]
    public IGitHubDownloadService GitHubDownloadService { get; init; } = null!;

    [UsedImplicitly]
    public IGitHubMirrorDownloadService GitHubMirrorDownloadService { get; init; } = null!;

    [UsedImplicitly]
    public Config Config { get; init; } = null!;

    #endregion Injections
}