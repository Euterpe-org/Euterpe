using System.Net.Http.Json;

namespace MuseDashModTools.Services;

public abstract class GitHubServiceBase
{
    private const string ReleaseApiUrl = "https://api.github.com/repos/MDModsDev/MuseDashModTools/releases";
    private const string LatestReleaseApiUrl = "https://api.github.com/repos/MDModsDev/MuseDashModTools/releases/latest";
    protected const string ModLinksBaseUrl = "MDModsDev/ModLinks/main/";
    protected const string MelonLoaderBaseUrl = "LavaGang/MelonLoader/releases/download/v0.6.1/MelonLoader.x64.zip";
    protected const string UnityDependencyBaseUrl = "LavaGang/Unity-Runtime-Libraries/master/2019.4.32.zip";

    protected const string Cpp2ILBaseUrl =
        "SamboyCoding/Cpp2IL/releases/download/2022.1.0-pre-release.10/Cpp2IL-2022.1.0-pre-release.10-Windows-Netframework472.zip";

    public abstract HttpClient Client { get; init; }

    public abstract ILogger Logger { get; init; }

    public abstract Setting Setting { get; init; }

    protected abstract Task DownloadAssetAsync(GithubRelease release, CancellationToken cancellationToken = default);

    protected async Task<GithubRelease?> GetLatestReleaseAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await Client.GetFromJsonAsync<GithubRelease>(LatestReleaseApiUrl, cancellationToken).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to fetch latest release from GitHub");
            return null;
        }
    }

    protected async Task<GithubRelease?> GetPrereleaseAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var releases = await Client.GetFromJsonAsync<GithubRelease[]>(ReleaseApiUrl, cancellationToken).ConfigureAwait(true);
            if (releases is not null)
            {
                return Array.Find(releases, x => x.Prerelease);
            }

            Logger.Warning("Fetched releases from GitHub is null");
            return null;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to fetch prerelease from GitHub");
            return null;
        }
    }

    protected async Task HandleReleaseAsync(GithubRelease? release, CancellationToken cancellationToken = default)
    {
        if (release is null)
        {
            Logger.Warning("Fetched release from GitHub is null");
            return;
        }

        var releaseVersion = SemVersion.Parse(release.TagName, SemVersionStyles.AllowV);
        Logger.Information("Release version parsed: {Version}", releaseVersion);

        if (releaseVersion.ComparePrecedenceTo(SemVersion.Parse(AppVersion)) <= 0 || Setting.SkipVersion == releaseVersion)
        {
            Logger.Information("No new version available");
            return;
        }

        var result = await NoticeConfirmMessageBoxAsync($"New version available: {releaseVersion}, do you want to upgrade?").ConfigureAwait(true);

        if (result == MessageBoxResult.Yes)
        {
            await DownloadAssetAsync(release, cancellationToken).ConfigureAwait(true);
            return;
        }

        Logger.Information("User choose to skip this version: {Version}", releaseVersion);
        Setting.SkipVersion = releaseVersion;
    }
}