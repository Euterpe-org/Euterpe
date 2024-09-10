namespace MuseDashModToolsUI.Services.Downloads;

public sealed class DownloadManager : IDownloadManager
{
    [UsedImplicitly]
    public IGitHubDownloadService GitHubDownloadService { get; init; } = null!;

    [UsedImplicitly]
    public IGitHubMirrorDownloadService GitHubMirrorDownloadService { get; init; } = null!;

    [UsedImplicitly]
    public ICustomDownloadService CustomDownloadService { get; init; } = null!;

    [UsedImplicitly]
    public Setting Setting { get; init; } = null!;

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

    public Task<bool> DownloadMelonLoaderAsync(CancellationToken cancellationToken = default)
    {
        return Setting.DownloadSource switch
        {
            DownloadSource.GitHub => GitHubDownloadService.DownloadMelonLoaderAsync(cancellationToken),
            DownloadSource.GitHubMirror => GitHubMirrorDownloadService.DownloadMelonLoaderAsync(cancellationToken),
            DownloadSource.Custom => CustomDownloadService.DownloadMelonLoaderAsync(cancellationToken),
            _ => throw new UnreachableException()
        };
    }
}