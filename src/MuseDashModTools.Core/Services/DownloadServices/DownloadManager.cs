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

    public Task<bool> DownloadLibAsync(LibDto lib, CancellationToken cancellationToken = default) =>
        CurrentDownloadService.DownloadLibAsync(lib, cancellationToken);

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

    public IAsyncEnumerable<Lib?> GetLibListAsync(CancellationToken cancellationToken = default) =>
        CurrentDownloadService.GetLibListAsync(cancellationToken);

    #region Injections

    [UsedImplicitly]
    public required Config Config { get; init; }

    [UsedImplicitly]
    public required ICustomDownloadService CustomDownloadService { get; init; }

    [UsedImplicitly]
    public required IGitHubDownloadService GitHubDownloadService { get; init; }

    [UsedImplicitly]
    public required IGitHubMirrorDownloadService GitHubMirrorDownloadService { get; init; }

    #endregion Injections
}