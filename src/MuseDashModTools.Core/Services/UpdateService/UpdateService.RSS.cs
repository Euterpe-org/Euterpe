using System.ServiceModel.Syndication;
using System.Xml;

namespace MuseDashModTools.Core;

internal sealed partial class UpdateService
{
    private async Task CheckGitHubRSSForUpdatesAsync(CancellationToken cancellationToken = default)
    {
        Logger.ZLogInformation($"Get Current version: {_currentVersion}");
        Logger.ZLogInformation($"Checking for updates from GitHub RSS...");

        GitHubRSS? release;
        if (!Config.DownloadPrerelease)
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
            var stream = await Client.GetStreamAsync(TagsRSSUrl, cancellationToken).ConfigureAwait(false);
            await using (stream.ConfigureAwait(false))
            {
                using var reader = XmlReader.Create(stream);
                return SyndicationFeed.Load(reader);
            }
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to fetch release from GitHub RSS");
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
            Logger.ZLogWarning($"Fetched release from GitHub RSS is null");
            return;
        }

        var releaseVersion = release.Version;
        Logger.ZLogInformation($"Release version parsed: {releaseVersion}");

        if (Config.SkipVersion == releaseVersion || releaseVersion.ComparePrecedenceTo(_currentVersion) <= 0)
        {
            Logger.ZLogInformation($"No new version available");
            return;
        }

        var result = await MessageBoxService.NoticeConfirmAsync($"New version available: {releaseVersion}, do you want to upgrade?")
            .ConfigureAwait(true);

        if (result is MessageBoxResult.Yes)
        {
            await DownloadManager.DownloadReleaseByTagAsync(release.Version.ToString(), PlatformService.OsString, cancellationToken).ConfigureAwait(false);
            Process.Start(
                new ProcessStartInfo
                {
                    FileName = PlatformService.UpdaterFileName,
                    UseShellExecute = true
                });
            return;
        }

        Logger.ZLogInformation($"User choose to skip this version: {releaseVersion}");
        Config.SkipVersion = releaseVersion;
    }
}