namespace MuseDashModToolsUI.Services;

public sealed class DownloadManager : IDownloadManager
{
    public Task<Mod[]?> GetModListAsync(CancellationToken cancellationToken = default)
    {
        return Setting.DownloadSource switch
        {
            DownloadSource.GitHub => GitHubDownloadService.GetModListAsync(cancellationToken),
            DownloadSource.GitHubMirror => GitHubMirrorDownloadService.GetModListAsync(cancellationToken),
            DownloadSource.Custom => CustomDownloadService.GetModListAsync(cancellationToken),
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