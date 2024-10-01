using System.Net.Http.Json;
using System.Text;

namespace MuseDashModTools.Services;

public abstract class GitHubServiceBase
{
    private const string ReleaseApiUrl = GitHubApiBaseUrl + "MDModsDev/MuseDashModTools/releases";
    private const string LatestReleaseApiUrl = GitHubApiBaseUrl + "MDModsDev/MuseDashModTools/releases/latest";
    protected const string ModLinksBaseUrl = "MDModsDev/ModLinks/main/";
    protected const string MelonLoaderBaseUrl = "LavaGang/MelonLoader/releases/download/v0.6.1/MelonLoader.x64.zip";
    protected const string UnityDependencyBaseUrl = "LavaGang/Unity-Runtime-Libraries/master/2019.4.32.zip";

    protected const string Cpp2ILBaseUrl =
        "SamboyCoding/Cpp2IL/releases/download/2022.1.0-pre-release.10/Cpp2IL-2022.1.0-pre-release.10-Windows-Netframework472.zip";

    protected readonly Dictionary<string, string> _readmeUrlCache = [];

    public abstract HttpClient Client { get; init; }

    public abstract ILogger Logger { get; init; }

    public abstract Setting Setting { get; init; }

    protected abstract Task DownloadAssetAsync(GitHubRelease release, CancellationToken cancellationToken = default);

    protected async Task<GitHubRelease?> GetLatestReleaseAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await Client.GetFromJsonAsync<GitHubRelease>(LatestReleaseApiUrl, cancellationToken).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to fetch latest release from GitHub");
            return null;
        }
    }

    protected async Task<GitHubRelease?> GetPrereleaseAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var releases = await Client.GetFromJsonAsync<GitHubRelease[]>(ReleaseApiUrl, cancellationToken).ConfigureAwait(true);
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

    protected async Task HandleReleaseAsync(GitHubRelease? release, CancellationToken cancellationToken = default)
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

    protected async Task<string?> FetchReadmeFromApiAsync(string repoId, CancellationToken cancellationToken = default)
    {
        var url = $"{GitHubApiBaseUrl}{repoId}/readme";

        try
        {
            var content = await Client.GetFromJsonAsync<ReadmeContent>(url, cancellationToken).ConfigureAwait(false);
            if (content is null)
            {
                return null;
            }

            Logger.Information("Successfully fetched Readme from API for {Repo}", repoId);
            return Encoding.UTF8.GetString(Convert.FromBase64String(content.Content));
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to fetch Readme from {Repo} using GitHub API", repoId);
            return null;
        }
    }
}