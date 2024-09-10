using System.Net.Http.Json;

namespace MuseDashModToolsUI.Services.Downloads;

public sealed class GitHubMirrorDownloadService : GitHubServiceBase, IGitHubMirrorDownloadService
{
    private const string PrimaryRawMirrorUrl = "https://raw.kkgithub.com/";
    private const string PrimaryReleaseMirrorUrl = "https://kkgithub.com/";
    private const string PrimaryModLinksUrl = PrimaryRawMirrorUrl + ModLinksBaseUrl + "ModLinks.json";
    private const string PrimaryMelonLoaderUrl = PrimaryReleaseMirrorUrl + MelonLoaderBaseUrl;

    [UsedImplicitly]
    public ILogger Logger { get; init; } = null!;

    [UsedImplicitly]
    public HttpClient Client { get; init; } = null!;

    public async Task<Mod[]?> GetModListAsync(CancellationToken cancellationToken = default)
    {
        Logger.Information("Fetching mods from GitHubMirror {Url} ...", PrimaryModLinksUrl);

        try
        {
            var mods = await Client.GetFromJsonAsync<Mod[]>(PrimaryModLinksUrl, cancellationToken).ConfigureAwait(false);
            Logger.Information("Mods fetched from GitHubMirror successfully");
            return mods;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to fetch mods from GitHubMirror");
            return null;
        }
    }
}