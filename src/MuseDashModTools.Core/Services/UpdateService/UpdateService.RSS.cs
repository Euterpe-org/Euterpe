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
            release = await GetStableReleaseFromRSSAsync(cancellationToken).ConfigureAwait(true);
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

    private async Task<GitHubRSS?> GetStableReleaseFromRSSAsync(CancellationToken cancellationToken = default)
    {
        var feed = await GetGitHubRSSFeedAsync(cancellationToken).ConfigureAwait(false);

        if (feed is null)
        {
            Logger.ZLogWarning($"Fetched stable release from GitHub RSS is null");
            return null;
        }

        foreach (var item in feed.Items)
        {
            var versionText = item.Title.Text;
            var version = SemVersion.Parse(versionText, SemVersionStyles.AllowV);

            if (!version.IsPrerelease)
            {
                return new GitHubRSS(version);
            }

            Logger.ZLogWarning($"Fetched stable release from GitHub RSS is a prerelease: {version}");
        }

        return null;
    }

    private async Task<GitHubRSS?> GetPrereleaseFromRSSAsync(CancellationToken cancellationToken = default)
    {
        var feed = await GetGitHubRSSFeedAsync(cancellationToken).ConfigureAwait(false);

        if (feed is null)
        {
            Logger.ZLogWarning($"Fetched prerelease from GitHub RSS is null");
            return null;
        }

        var release = feed.Items.First();
        return new GitHubRSS(SemVersion.Parse(release.Title.Text));
    }

    private async Task HandleRSSReleaseAsync(GitHubRSS? release, CancellationToken cancellationToken = default)
    {
        if (release is null)
        {
            return;
        }

        var releaseVersion = release.Version;
        Logger.ZLogInformation($"Release version parsed: {releaseVersion}");

        var shouldUpdate = await ShouldUpdateAsync(releaseVersion).ConfigureAwait(false);
        if (!shouldUpdate)
        {
            return;
        }

        await StartUpdateProcessAsync(releaseVersion.ToString(), cancellationToken).ConfigureAwait(false);
        Environment.Exit(0);
    }
}