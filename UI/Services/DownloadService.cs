using System.Net.Http.Json;

#pragma warning disable CS8618

namespace MuseDashModToolsUI.Services;

public sealed partial class DownloadService : IDownloadService
{
    // Github Release APIs
    private const string ReleaseInfoLink = "https://api.github.com/repos/MDModsDev/MuseDashModToolsUI/releases";

    // Chart APIs
    private const string RankedChartListApi = "https://mdmc.moe/api/v1/charts";
    private const string UnrankedChartListApi = "https://mdmc.moe/api/v1/charts/unranked";
    private const string ChartCoverApi = "https://mdmc.moe/charts/{0}/cover.png";
    private const string ChartDownloadApi = "https://mdmc.moe/download/{0}";

    // ModLinks
    private const string PrimaryLink = "https://raw.githubusercontent.com/MDModsDev/ModLinks/main/";
    private const string SecondaryLink = "https://mirror.ghproxy.com/https://raw.githubusercontent.com/MDModsDev/ModLinks/main/";
    private const string ThirdLink = "https://gitee.com/lxymahatma/ModLinks/raw/main/";

    private HttpResponseMessage? _melonLoaderResponseMessage;
    private string DefaultDownloadSource => DownloadSourceDictionary[Settings.DownloadSource];

    private Dictionary<DownloadSources, string> DownloadSourceDictionary => new()
    {
        { DownloadSources.Github, PrimaryLink },
        { DownloadSources.GithubMirror, SecondaryLink },
        { DownloadSources.Gitee, ThirdLink },
        { DownloadSources.Custom, Settings.CustomDownloadSource! }
    };

    public async Task CheckUpdatesAsync(bool isUserClick = false)
    {
        Logger.Information("Checking updates...");
        Client.DefaultRequestHeaders.Add("User-Agent", Environment.UserName);

        Logger.Information("Get current version success: {Version}", AppVersion);
        try
        {
            var releases = await Client.GetFromJsonAsync<List<GithubRelease>>(ReleaseInfoLink);
            Logger.Information("Get releases success");

            var release = Settings.DownloadPrerelease ? releases![0] : releases!.Find(x => !x.Prerelease)!;

            var version = GetVersionFromTag(release.TagName);
            if (version is null || await SkipVersionCheck(version, AppVersion, isUserClick))
            {
                return;
            }

            var title = release.Name;
            var body = release.Body;
            if (!await UpdateRequired(release.TagName, title, body))
            {
                return;
            }

            var link = GetDownloadLink(release.Assets);
            if (!await LocalService.Value.LaunchUpdaterAsync(link))
            {
                return;
            }

            Logger.Information("Launch updater success, exit...");
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Check updates failed");
            await MessageBoxService.ErrorMessageBox(MsgBox_Content_CheckUpdateFailed);
        }
    }

    public async Task DownloadChartAsync(int id, string path)
    {
        var url = ZString.Format(ChartDownloadApi, id);
        try
        {
            var result = await Client.GetAsync(url);
            await using var fs = new FileStream(path, FileMode.OpenOrCreate);
            await result.Content.CopyToAsync(fs);
            Logger.Information("Download chart from {Url} success", url);
        }
        catch (Exception ex)
        {
            Logger.Warning("Download chart from {Url} failed", url);
            await MessageBoxService.ErrorMessageBox(MsgBox_Content_DownloadChartFailed, ex);
        }
    }

    public async Task DownloadModAsync(string link, string path)
    {
        var defaultDownloadSource = DownloadSourceDictionary[Settings.DownloadSource];
        var result = await DownloadModFromSourceAsync(defaultDownloadSource, link, path);
        if (result is not null)
        {
            return;
        }

        foreach (var pair in DownloadSourceDictionary.Where(pair => pair.Key != Settings.DownloadSource))
        {
            result = await DownloadModFromSourceAsync(pair.Value, link, path);
            if (result is not null)
            {
                return;
            }
        }
    }

    public async Task<bool> DownloadMelonLoaderAsync(IProgress<double> downloadProgress)
    {
        if (_melonLoaderResponseMessage is null)
        {
            await GetMelonLoaderResponseMessage();
        }

        if (_melonLoaderResponseMessage is null)
        {
            return false;
        }

        return await DownloadMelonLoaderFromSourceAsync(downloadProgress);
    }

    public async Task<List<Chart>?> GetChartListAsync()
    {
        try
        {
            var charts = await Client.GetFromJsonAsync<List<Chart>>(RankedChartListApi);
            await GetChartCovers(charts);
            Logger.Information("Get chart list success");
            return charts;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Get chart list failed");
            return null;
        }
    }

    public async Task<long?> GetMelonLoaderFileSizeAsync()
    {
        await GetMelonLoaderResponseMessage();
        return _melonLoaderResponseMessage is not null ? _melonLoaderResponseMessage.Content.Headers.ContentLength : 0;
    }

    public async Task<List<Mod>?> GetModListAsync()
    {
        var mods = await GetModListFromSourceAsync(DefaultDownloadSource);
        if (mods is not null)
        {
            return mods;
        }

        foreach (var pair in DownloadSourceDictionary.Where(pair => pair.Key != Settings.DownloadSource))
        {
            mods = await GetModListFromSourceAsync(pair.Value);
            if (mods is not null)
            {
                return mods;
            }
        }

        if (File.Exists(Settings.ModLinksPath))
        {
            Logger.Information("Get mod list from online source failed, deserializing local mod list");
            mods = await SerializationService.DeserializeModListAsync();
            if (mods is not null)
            {
                return mods;
            }
        }

        Logger.Error("Failed to get working mod list from any source");
        await MessageBoxService.ErrorMessageBox(MsgBox_Content_GetModListFailed);
        return null;
    }

    #region Services

    [UsedImplicitly]
    public HttpClient Client { get; init; }

    [UsedImplicitly]
    public ILogger Logger { get; init; }

    [UsedImplicitly]
    public IMessageBoxService MessageBoxService { get; init; }

    [UsedImplicitly]
    public IPlatformService PlatformService { get; init; }

    [UsedImplicitly]
    public ISerializationService SerializationService { get; init; }

    [UsedImplicitly]
    public Lazy<ILocalService> LocalService { get; init; }

    [UsedImplicitly]
    public Setting Settings { get; init; }

    #endregion
}