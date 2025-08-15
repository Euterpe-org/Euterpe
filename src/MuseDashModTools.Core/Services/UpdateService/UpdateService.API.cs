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
            return await Client.GetFromJsonAsync<GitHubRelease>(LatestReleaseAPIUrl, Default.GitHubRelease, cancellationToken)
                .ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to fetch latest release from GitHub API");
            return null;
        }
    }

    private async Task<GitHubRelease?> GetPrereleaseFromAPIAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var releases = await Client.GetFromJsonAsync<GitHubRelease[]>(ReleaseAPIUrl, Default.GitHubReleaseArray, cancellationToken).ConfigureAwait(true);
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
            Logger.ZLogWarning($"Fetched release from GitHub API is null");
            return;
        }

        var releaseVersion = SemVersion.Parse(release.TagName, SemVersionStyles.AllowV);
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
            await DownloadManager.DownloadReleaseByTagAsync(release.TagName, PlatformService.OsString, cancellationToken).ConfigureAwait(false);
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