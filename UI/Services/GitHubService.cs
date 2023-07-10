using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MsBox.Avalonia.Enums;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Extensions;
using MuseDashModToolsUI.Models;
using Serilog;
using static MuseDashModToolsUI.Localization.Resources;

#pragma warning disable CS8618

namespace MuseDashModToolsUI.Services;

public class GitHubService : IGitHubService
{
    private const string ReleaseInfoLink = "https://api.github.com/repos/MDModsDev/MuseDashModToolsUI/releases/latest";

    private const string PrimaryLink = "https://raw.githubusercontent.com/MDModsDev/ModLinks/main/";

    private const string SecondaryLink = "https://ghproxy.com/https://raw.githubusercontent.com/MDModsDev/ModLinks/main/";

    private const string ThirdLink = "https://gitee.com/lxymahatma/ModLinks/raw/main/";

    private static Dictionary<DownloadSources, string> DownloadSourceDictionary => new()
    {
        { DownloadSources.Github, PrimaryLink },
        { DownloadSources.GithubMirror, SecondaryLink },
        { DownloadSources.Gitee, ThirdLink }
    };

    public HttpClient Client { get; init; }
    public ILogger Logger { get; init; }
    public IMessageBoxService MessageBoxService { get; init; }
    public ISettingService SettingService { get; init; }

    public async Task<List<Mod>?> GetModListAsync()
    {
        var defaultDownloadSource = DownloadSourceDictionary[SettingService.Settings.DownloadSource];
        var mods = await GetModListFromSourceAsync(defaultDownloadSource);
        if (mods is not null) return mods;

        foreach (var pair in DownloadSourceDictionary.Where(pair => pair.Key != SettingService.Settings.DownloadSource))
        {
            mods = await GetModListFromSourceAsync(pair.Value);
            if (mods is not null) return mods;
        }

        await MessageBoxService.CreateErrorMessageBox(MsgBox_Content_GetModListFailed);
        return null;
    }

    public async Task DownloadModAsync(string link, string path)
    {
        var defaultDownloadSource = DownloadSourceDictionary[SettingService.Settings.DownloadSource];
        var result = await DownloadModFromSourceAsync(defaultDownloadSource, link, path);
        if (result is not null) return;

        foreach (var pair in DownloadSourceDictionary.Where(pair => pair.Key != SettingService.Settings.DownloadSource))
        {
            result = await DownloadModFromSourceAsync(pair.Value, link, path);
            if (result is not null) return;
        }
    }

    public async Task DownloadMelonLoader(string path, IProgress<double> downloadProgress)
    {
        var defaultDownloadSource = DownloadSourceDictionary[SettingService.Settings.DownloadSource];
        var result = await DownloadMelonLoaderFromSource(defaultDownloadSource, path, downloadProgress);
        if (result is not null) return;

        foreach (var pair in DownloadSourceDictionary.Where(pair => pair.Key != SettingService.Settings.DownloadSource))
        {
            result = await DownloadMelonLoaderFromSource(pair.Value, path, downloadProgress);
            if (result is not null) return;
        }
    }

    public async Task CheckUpdates(bool userClick = false)
    {
        Logger.Information("Checking updates...");
        Client.DefaultRequestHeaders.Add("User-Agent", "MuseDashModToolsUI");

        var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
        Logger.Information("Get current version success: {Version}", currentVersion);
        try
        {
            var result = await Client.GetStringAsync(ReleaseInfoLink);
            var doc = JsonDocument.Parse(result);
            if (!doc.RootElement.TryGetProperty("tag_name", out var tagName)) return;

            var version = GetVersionFromTag(tagName);
            if (version is null || await SkipVersionCheck(version, currentVersion!, userClick)) return;

            var title = doc.RootElement.GetProperty("name").GetString();
            var body = doc.RootElement.GetProperty("body").GetString();
            if (!await UpdateRequired(version, title!, body!)) return;

            var link = GetDownloadLink(doc);
            var currentDirectory = Directory.GetCurrentDirectory();
            await LaunchUpdater(currentDirectory, new[] { link, currentDirectory + ".zip", currentDirectory });
            Logger.Information("Launch updater success, exit...");
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Check updates failed");
            await MessageBoxService.CreateErrorMessageBox(MsgBox_Content_CheckUpdateFailed.Localize());
        }
    }

    private Version? GetVersionFromTag(JsonElement tagName)
    {
        var tag = tagName.GetString();
        if (tag is null) return null;

        if (!Version.TryParse(tag.StartsWith('v') ? tag[1..] : tag, out var version)) return null;
        Logger.Information("Get latest version success: {Version}", version);

        return version;
    }

    private async Task<bool> SkipVersionCheck(Version version, Version currentVersion, bool userClick)
    {
        if (!userClick) return version == SettingService.Settings.SkipVersion || version <= currentVersion;
        if (version > currentVersion) return false;
        await MessageBoxService.CreateSuccessMessageBox(MsgBox_Content_LatestVersion.Localize());
        return true;
    }

    private async Task<bool> UpdateRequired(Version version, string title, string body)
    {
        var update = await MessageBoxService.CreateCustomConfirmMessageBox(MsgBox_Title_Notice,
            string.Format(MsgBox_Content_NewerVersion.Localize(), version, title, body), 3, Icon.Info);

        if (update == MsgBox_Button_NoNoAsk)
        {
            SettingService.Settings.SkipVersion = version;
            return false;
        }

        return update != MsgBox_Button_No;
    }

    private string GetDownloadLink(JsonDocument doc)
    {
        var assets = doc.RootElement.GetProperty("assets");
        var releases = assets.EnumerateArray();

        var osString = OperatingSystem.IsWindows() ? "Windows" : "Linux";

        var release = releases.FirstOrDefault(x =>
            x.GetProperty("name").GetString()!.Contains(osString, StringComparison.OrdinalIgnoreCase));
        var link = release.GetProperty("browser_download_url").GetString()!;

        Logger.Information("Get {OsString} download link success {Link}", osString, link);
        return link;
    }

    private async Task<HttpResponseMessage?> DownloadMelonLoaderFromSource(string downloadSource, string path,
        IProgress<double> downloadProgress)
    {
        var url = downloadSource + "MelonLoader.zip";
        try
        {
            var result = await Client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            Logger.Information("Get MelonLoader Download ResponseHeader from {Url} success", url);

            var totalLength = result.Content.Headers.ContentLength;
            var contentStream = await result.Content.ReadAsStreamAsync();
            await using var fs = new FileStream(path, FileMode.OpenOrCreate);
            var buffer = new byte[5 * 1024];
            var readLength = 0L;
            int length;
            while ((length = await contentStream.ReadAsync(buffer.AsMemory(0, buffer.Length), CancellationToken.None)) != 0)
            {
                readLength += length;
                if (totalLength > 0) downloadProgress.Report(Math.Round((double)readLength / totalLength.Value * 100, 2));

                fs.Write(buffer, 0, length);
            }

            Logger.Information("Download MelonLoader success");
            return result;
        }
        catch
        {
            Logger.Warning("Download MelonLoader from {Url} failed", url);
            return null;
        }
    }

    private async Task<HttpResponseMessage?> DownloadModFromSourceAsync(string downloadSource, string link, string path)
    {
        var url = downloadSource + link;
        try
        {
            var result = await Client.GetAsync(url);
            Logger.Information("Download mod from {Url} success", url);
            await using var fs = new FileStream(path, FileMode.OpenOrCreate);
            await result.Content.CopyToAsync(fs);
            return result;
        }
        catch
        {
            Logger.Warning("Download mod from {Url} failed", url);
            return null;
        }
    }

    private async Task<List<Mod>?> GetModListFromSourceAsync(string downloadSource)
    {
        var url = downloadSource + "ModLinks.json";
        try
        {
            var mods = (await Client.GetFromJsonAsync<List<Mod>>(url))!;
            Logger.Information("Get mod list from {Url} success", url);
            return mods;
        }
        catch
        {
            Logger.Warning("Get mod list from {Url} failed", url);
            return null;
        }
    }

    private async Task LaunchUpdater(string currentDirectory, IEnumerable<string> launchArgs)
    {
        var updaterExePath = Path.Combine(currentDirectory, "Updater.exe");
        var updaterTargetFolder = Path.Combine(currentDirectory, "Update");
        var updaterTargetPath = Path.Combine(currentDirectory, "Update", "Updater.exe");
        if (!File.Exists(updaterExePath))
        {
            Logger.Error("Updater.exe not found");
            await MessageBoxService.CreateErrorMessageBox(MsgBox_Content_UpdaterNotFound.Localize());
            return;
        }

        if (!Directory.Exists(updaterTargetFolder))
        {
            Directory.CreateDirectory(updaterTargetFolder);
            Logger.Information("Create Update target folder success");
        }

        try
        {
            File.Copy(updaterExePath, updaterTargetPath, true);
            Logger.Information("Copy Updater.exe to Update folder success");
        }
        catch (Exception ex)
        {
            Logger.Information("Copy Updater.exe to Update folder failed: {Exception}", ex.ToString());
            await MessageBoxService.CreateErrorMessageBox(
                string.Format(MsgBox_Content_CopyUpdaterFailed.Localize(), ex));
        }

        Process.Start(updaterTargetPath, launchArgs);
    }
}