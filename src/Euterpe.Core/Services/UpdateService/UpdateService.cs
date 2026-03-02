namespace Euterpe.Core;

internal sealed partial class UpdateService : IUpdateService
{
    private const string ReleaseAPIUrl = GitHubAPIBaseUrl + ModToolsRepoIdentifier + "releases";
    private const string LatestReleaseAPIUrl = GitHubAPIBaseUrl + ModToolsRepoIdentifier + "releases/latest";
    private const string TagsRSSUrl = GitHubBaseUrl + ModToolsRepoIdentifier + "releases.atom";
    private const string GeoDetectionUrl = "https://ipinfo.io/country";

    private static readonly SemVersion _currentVersion = SemVersion.Parse(AppVersion);

    public Task<bool> CheckForUpdatesAsync(CancellationToken cancellationToken = default)
    {
        return Config.UpdateSource switch
        {
            UpdateSource.GitHubAPI => CheckGitHubAPIForUpdatesAsync(cancellationToken),
            UpdateSource.GitHubRSS => CheckGitHubRSSForUpdatesAsync(cancellationToken),
            _ => throw new UnreachableException()
        };
    }

    public async Task ConfigureDownloadSourceAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var country = await Client.GetStringAsync(GeoDetectionUrl, cancellationToken).ConfigureAwait(false);
            var downloadSource = country.Trim().Equals("CN", StringComparison.OrdinalIgnoreCase)
                ? DownloadSource.Gitee
                : DownloadSource.GitHub;

            Config.DownloadSource = downloadSource;
            Logger.ZLogInformation($"Download source configured to {downloadSource} based on geo-detection");
        }
        catch (Exception ex)
        {
            Logger.ZLogWarning(ex, $"Failed to detect geo location, keeping current download source");
        }
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

    [UsedImplicitly]
    public required IPlatformService PlatformService { get; init; }

    #endregion Injections
}