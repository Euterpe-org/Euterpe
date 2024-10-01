namespace MuseDashModTools.Services;

public sealed partial class DownloadManager : IDownloadManager
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

    public Task<string?> FetchReadmeAsync(string repoId, CancellationToken cancellationToken = default)
    {
        return Setting.DownloadSource switch
        {
            DownloadSource.GitHub => GitHubDownloadService.FetchReadmeAsync(repoId, cancellationToken),
            DownloadSource.GitHubMirror => GitHubMirrorDownloadService.FetchReadmeAsync(repoId, cancellationToken),
            // For Custom Download Source, because they don't choose GitHub for other downloads, so we will use GitHubMirror
            DownloadSource.Custom => GitHubMirrorDownloadService.FetchReadmeAsync(repoId, cancellationToken),
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