namespace MuseDashModTools.Core;

internal sealed partial class UpdateService : IUpdateService
{
    private const string ReleaseAPIUrl = GitHubAPIBaseUrl + RepoIdentifier + "releases";
    private const string LatestReleaseAPIUrl = GitHubAPIBaseUrl + RepoIdentifier + "releases/latest";
    private const string TagsRSSUrl = GitHubBaseUrl + RepoIdentifier + "tags.atom";

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
    public HttpClient Client { get; init; } = null!;

    [UsedImplicitly]
    public Config Config { get; init; } = null!;

    [UsedImplicitly]
    public IDownloadManager DownloadManager { get; init; } = null!;

    [UsedImplicitly]
    public ILogger<UpdateService> Logger { get; init; } = null!;

    [UsedImplicitly]
    public IMessageBoxService MessageBoxService { get; init; } = null!;

    #endregion Injections
}