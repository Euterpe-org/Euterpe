namespace MuseDashModTools.Core;

internal sealed partial class UpdateService : IUpdateService
{
    private const string ReleaseAPIUrl = GitHubAPIBaseUrl + ModToolsRepoIdentifier + "releases";
    private const string LatestReleaseAPIUrl = GitHubAPIBaseUrl + ModToolsRepoIdentifier + "releases/latest";
    private const string TagsRSSUrl = GitHubBaseUrl + ModToolsRepoIdentifier + "tags.atom";

    private static readonly SemVersion _currentVersion = SemVersion.Parse(AppVersion);

    public Task CheckForUpdatesAsync(CancellationToken cancellationToken = default)
    {
        return Config.UpdateSource switch
        {
            UpdateSource.GitHubAPI => CheckGitHubAPIForUpdatesAsync(cancellationToken),
            UpdateSource.GitHubRSS => CheckGitHubRSSForUpdatesAsync(cancellationToken),
            _ => throw new UnreachableException()
        };
    }

    #region Injections

    [UsedImplicitly]
    public required HttpClient Client { get; init; }

    [UsedImplicitly]
    public required Config Config { get; init; }

    [UsedImplicitly]
    public required IDownloadManager DownloadManager { get; init; }

    [UsedImplicitly]
    public required ILogger<UpdateService> Logger { get; init; }

    [UsedImplicitly]
    public required IMessageBoxService MessageBoxService { get; init; }

    #endregion Injections
}