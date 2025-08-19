using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MuseDashModTools.Core;

internal sealed partial class UpdateService
{
    private async Task CheckGitHubAPIForUpdatesAsync(CancellationToken cancellationToken = default)
    {
        Logger.ZLogInformation($"Get Current version: {_currentVersion}");
        Logger.ZLogInformation($"Checking for updates from GitHub API...");

        Client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(AppName, AppVersion));
        Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        if (!Config.GitHubToken.IsNullOrEmpty())
        {
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", Config.GitHubToken);
        }

        GitHubRelease? release;
        if (!Config.DownloadPrerelease)
        {
            release = await GetStableReleaseFromAPIAsync(cancellationToken).ConfigureAwait(true);
        }
        else
        {
            release = await GetPrereleaseFromAPIAsync(cancellationToken).ConfigureAwait(true);
        }

        await HandleAPIReleaseAsync(release, cancellationToken).ConfigureAwait(true);
    }

    private async Task<GitHubRelease?> GetStableReleaseFromAPIAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var release = await Client.GetFromJsonAsync<GitHubRelease>(LatestReleaseAPIUrl, Default.GitHubRelease, cancellationToken).ConfigureAwait(false);

            if (release is not null)
            {
                var version = SemVersion.Parse(release.TagName, SemVersionStyles.AllowV);
                if (!version.IsPrerelease)
                {
                    return release;
                }

                Logger.ZLogWarning($"Fetched stable release from GitHub API is a prerelease: {version}");
                return null;
            }

            Logger.ZLogWarning($"Fetched stable release from GitHub API is null");
            return null;
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to fetch stable release from GitHub API");
            return null;
        }
    }

    private async Task<GitHubRelease?> GetPrereleaseFromAPIAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var releases = await Client.GetFromJsonAsync<GitHubRelease[]>(ReleaseAPIUrl, Default.GitHubReleaseArray, cancellationToken).ConfigureAwait(false);
            if (releases is not (null or []))
            {
                return releases[0];
            }

            Logger.ZLogWarning($"Fetched releases from GitHub API is null");
            return null;
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to fetch prerelease from GitHub API");
            return null;
        }
    }

    private async Task HandleAPIReleaseAsync(GitHubRelease? release, CancellationToken cancellationToken = default)
    {
        if (release is null)
        {
            return;
        }

        var releaseVersion = SemVersion.Parse(release.TagName, SemVersionStyles.AllowV);
        Logger.ZLogInformation($"Release version parsed: {releaseVersion}");

        var shouldUpdate = await ShouldUpdateAsync(releaseVersion).ConfigureAwait(false);
        if (!shouldUpdate)
        {
            return;
        }

        await StartUpdateProcessAsync(release.TagName, cancellationToken).ConfigureAwait(false);
        Environment.Exit(0);
    }
}