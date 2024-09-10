using System.Net.Http.Json;

namespace MuseDashModToolsUI.Services.Downloads;

public sealed class GitHubDownloadService : GitHubServiceBase, IGitHubDownloadService
{
    private const string RawGitHubUrl = "https://raw.githubusercontent.com/";
    private const string ReleaseGitHubUrl = "https://github.com/";
    private const string ModLinksUrl = RawGitHubUrl + ModLinksBaseUrl + "ModLinks.json";
    private const string MelonLoaderUrl = ReleaseGitHubUrl + MelonLoaderBaseUrl;

    [UsedImplicitly]
    public HttpClient Client { get; init; } = null!;

    [UsedImplicitly]
    public ILogger Logger { get; init; } = null!;

    public async Task<Mod[]?> GetModListAsync(CancellationToken cancellationToken = default)
    {
        Logger.Information("Fetching mods from GitHub {Url}...", ModLinksUrl);

        try
        {
            var mods = await Client.GetFromJsonAsync<Mod[]>(ModLinksUrl, cancellationToken).ConfigureAwait(false);
            Logger.Information("Mods fetched from GitHub successfully");
            return mods;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to fetch mods from GitHub");
            return null;
        }
    }
}