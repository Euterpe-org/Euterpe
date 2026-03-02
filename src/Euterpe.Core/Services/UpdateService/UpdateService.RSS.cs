using System.ServiceModel.Syndication;
using System.Xml;

namespace Euterpe.Core;

internal sealed partial class UpdateService
{
    private async Task<SyndicationFeed?> GetRSSFeedAsync(CancellationToken cancellationToken = default)
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
            Logger.ZLogError(ex, $"Failed to fetch release from RSS");
            return null;
        }
    }

    private async Task<ReleaseInfo?> GetStableReleaseFromRSSAsync(CancellationToken cancellationToken = default)
    {
        var feed = await GetRSSFeedAsync(cancellationToken).ConfigureAwait(false);

        if (feed is null)
        {
            Logger.ZLogWarning($"Fetched stable release from RSS is null");
            return null;
        }

        foreach (var item in feed.Items)
        {
            var versionText = item.Title.Text;
            var version = SemVersion.Parse(versionText);

            if (!version.IsPrerelease)
            {
                return new ReleaseInfo(version);
            }

            Logger.ZLogWarning($"Fetched stable release from RSS is a prerelease: {version}");
        }

        return null;
    }

    private async Task<ReleaseInfo?> GetPrereleaseFromRSSAsync(CancellationToken cancellationToken = default)
    {
        var feed = await GetRSSFeedAsync(cancellationToken).ConfigureAwait(false);

        if (feed is null)
        {
            Logger.ZLogWarning($"Fetched prerelease from RSS is null");
            return null;
        }

        var release = feed.Items.First();
        return new ReleaseInfo(SemVersion.Parse(release.Title.Text));
    }

    private async Task<bool> HandleRSSReleaseAsync(ReleaseInfo? release, CancellationToken cancellationToken = default)
    {
        if (release is null)
        {
            return false;
        }

        var releaseVersion = release.Version;
        Logger.ZLogInformation($"Release version parsed: {releaseVersion}");

        var shouldUpdate = await ShouldUpdateAsync(releaseVersion).ConfigureAwait(false);
        if (!shouldUpdate)
        {
            return false;
        }

        await StartUpdateProcessAsync(releaseVersion.ToString(), cancellationToken).ConfigureAwait(false);
        Environment.Exit(0);
        return true;
    }
}