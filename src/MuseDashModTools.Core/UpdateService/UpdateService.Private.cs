using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.ServiceModel.Syndication;
using System.Xml;

namespace MuseDashModTools.Core.UpdateService;

public sealed partial class UpdateService
{
    #region GitHub RSS

    private async Task CheckGitHubRSSForUpdatesAsync(CancellationToken cancellationToken = default)
    {
        Logger.Information("Get Current version: {Version}", _currentVersion);
        Logger.Information("Checking for updates from GitHub RSS...");

        GitHubRSS? release;
        if (!Setting.DownloadPrerelease)
        {
            release = await GetLatestReleaseFromRSSAsync(cancellationToken).ConfigureAwait(true);
        }
        else
        {
            release = await GetPrereleaseFromRSSAsync(cancellationToken).ConfigureAwait(true);
        }

        await HandleRSSReleaseAsync(release, cancellationToken).ConfigureAwait(true);
    }

    private async Task<SyndicationFeed?> GetGitHubRSSFeedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var stream = await Client.GetStreamAsync(TagsRSSUrl, cancellationToken).ConfigureAwait(false);
            using var reader = XmlReader.Create(stream);
            return SyndicationFeed.Load(reader);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to fetch release from GitHub RSS");
            return null;
        }
    }

    private async Task<GitHubRSS?> GetLatestReleaseFromRSSAsync(CancellationToken cancellationToken = default)
    {
        var feed = await GetGitHubRSSFeedAsync(cancellationToken).ConfigureAwait(false);

        if (feed is null)
        {
            return null;
        }

        foreach (var item in feed.Items)
        {
            var versionText = item.Title.Text;

            if (SemVersion.TryParse(versionText, SemVersionStyles.AllowV, out var version) && !version.IsPrerelease)
            {
                return new GitHubRSS(version);
            }
        }

        return null;
    }

    private async Task<GitHubRSS?> GetPrereleaseFromRSSAsync(CancellationToken cancellationToken = default)
    {
        var feed = await GetGitHubRSSFeedAsync(cancellationToken).ConfigureAwait(false);

        if (feed is null)
        {
            return null;
        }

        var release = feed.Items.First();
        return SemVersion.TryParse(release.Title.Text, SemVersionStyles.AllowV, out var version)
            ? new GitHubRSS(version)
            : null;
    }

    private async Task HandleRSSReleaseAsync(GitHubRSS? release, CancellationToken cancellationToken = default)
    {
        if (release is null)
        {
            Logger.Warning("Fetched release from GitHub RSS is null");
            return;
        }

        var releaseVersion = release.Version;
        Logger.Information("Release version parsed: {Version}", releaseVersion);

        if (Setting.SkipVersion == releaseVersion || releaseVersion.ComparePrecedenceTo(_currentVersion) <= 0)
        {
            Logger.Information("No new version available");
            return;
        }

        var result = await MessageBoxService.NoticeConfirmMessageBoxAsync($"New version available: {releaseVersion}, do you want to upgrade?")
            .ConfigureAwait(true);

        if (result == MessageBoxResult.Yes)
        {
            await DownloadManager.DownloadReleaseByTagAsync(release.Version.ToString(), cancellationToken).ConfigureAwait(true);
            return;
        }

        Logger.Information("User choose to skip this version: {Version}", releaseVersion);
        Setting.SkipVersion = releaseVersion;
    }

    #endregion GitHub RSS

    #region GitHub API

    private async Task CheckGitHubAPIForUpdatesAsync(CancellationToken cancellationToken = default)
    {
        Logger.Information("Get Current version: {Version}", _currentVersion);
        Logger.Information("Checking for updates from GitHub API...");

        Client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(AppName, AppVersion));
        Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        if (!Setting.GitHubToken.IsNullOrEmpty())
        {
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", Setting.GitHubToken);
        }

        GitHubRelease? release;
        if (!Setting.DownloadPrerelease)
        {
            release = await GetLatestReleaseFromAPIAsync(cancellationToken).ConfigureAwait(true);
        }
        else
        {
            release = await GetPrereleaseFromAPIAsync(cancellationToken).ConfigureAwait(true);
        }

        await HandleAPIReleaseAsync(release, cancellationToken).ConfigureAwait(true);
    }

    private async Task<GitHubRelease?> GetLatestReleaseFromAPIAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await Client.GetFromJsonAsync<GitHubRelease>(LatestReleaseAPIUrl, cancellationToken).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to fetch latest release from GitHub API");
            return null;
        }
    }

    private async Task<GitHubRelease?> GetPrereleaseFromAPIAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var releases = await Client.GetFromJsonAsync<GitHubRelease[]>(ReleaseAPIUrl, cancellationToken).ConfigureAwait(true);
            if (releases is not (null or []))
            {
                return releases[0];
            }

            Logger.Warning("Fetched releases from GitHub API is null");
            return null;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to fetch prerelease from GitHub API");
            return null;
        }
    }

    private async Task HandleAPIReleaseAsync(GitHubRelease? release, CancellationToken cancellationToken = default)
    {
        if (release is null)
        {
            Logger.Warning("Fetched release from GitHub API is null");
            return;
        }

        var releaseVersion = SemVersion.Parse(release.TagName, SemVersionStyles.AllowV);
        Logger.Information("Release version parsed: {Version}", releaseVersion);

        if (Setting.SkipVersion == releaseVersion || releaseVersion.ComparePrecedenceTo(_currentVersion) <= 0)
        {
            Logger.Information("No new version available");
            return;
        }

        var result = await MessageBoxService.NoticeConfirmMessageBoxAsync($"New version available: {releaseVersion}, do you want to upgrade?")
            .ConfigureAwait(true);

        if (result == MessageBoxResult.Yes)
        {
            await DownloadManager.DownloadReleaseByTagAsync(release.TagName, cancellationToken).ConfigureAwait(true);
            return;
        }

        Logger.Information("User choose to skip this version: {Version}", releaseVersion);
        Setting.SkipVersion = releaseVersion;
    }

    #endregion GitHub API
}