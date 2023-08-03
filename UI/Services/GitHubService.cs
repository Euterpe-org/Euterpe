using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Extensions;
using MuseDashModToolsUI.Models;
using NuGet.Versioning;
using Serilog;
using static MuseDashModToolsUI.Localization.Resources;

#pragma warning disable CS8618

namespace MuseDashModToolsUI.Services;

public partial class GitHubService : IGitHubService
{
    private const string ReleaseInfoLink = "https://api.github.com/repos/MDModsDev/MuseDashModToolsUI/releases";

    private const string PrimaryLink = "https://raw.githubusercontent.com/MDModsDev/ModLinks/main/";

    private const string SecondaryLink = "https://ghproxy.com/https://raw.githubusercontent.com/MDModsDev/ModLinks/main/";

    private const string ThirdLink = "https://gitee.com/lxymahatma/ModLinks/raw/main/";
    private string DefaultDownloadSource => DownloadSourceDictionary[SavingService.Settings.DownloadSource];

    private static Dictionary<DownloadSources, string> DownloadSourceDictionary => new()
    {
        { DownloadSources.Github, PrimaryLink },
        { DownloadSources.GithubMirror, SecondaryLink },
        { DownloadSources.Gitee, ThirdLink }
    };

    public HttpClient Client { get; init; }
    public ILogger Logger { get; init; }
    public IMessageBoxService MessageBoxService { get; init; }
    public ISavingService SavingService { get; init; }

    public async Task<List<Mod>?> GetModListAsync()
    {
        var mods = await GetModListFromSourceAsync(DefaultDownloadSource);
        if (mods is not null) return mods;

        foreach (var pair in DownloadSourceDictionary.Where(pair => pair.Key != SavingService.Settings.DownloadSource))
        {
            mods = await GetModListFromSourceAsync(pair.Value);
            if (mods is not null) return mods;
        }

        await MessageBoxService.CreateErrorMessageBox(MsgBox_Content_GetModListFailed);
        return null;
    }

    public async Task DownloadModAsync(string link, string path)
    {
        var defaultDownloadSource = DownloadSourceDictionary[SavingService.Settings.DownloadSource];
        var result = await DownloadModFromSourceAsync(defaultDownloadSource, link, path);
        if (result is not null) return;

        foreach (var pair in DownloadSourceDictionary.Where(pair => pair.Key != SavingService.Settings.DownloadSource))
        {
            result = await DownloadModFromSourceAsync(pair.Value, link, path);
            if (result is not null) return;
        }
    }

    public async Task DownloadMelonLoader(string path, IProgress<double> downloadProgress)
    {
        var result = await DownloadMelonLoaderFromSource(DefaultDownloadSource, path, downloadProgress);
        if (result is not null) return;

        foreach (var pair in DownloadSourceDictionary.Where(pair => pair.Key != SavingService.Settings.DownloadSource))
        {
            result = await DownloadMelonLoaderFromSource(pair.Value, path, downloadProgress);
            if (result is not null) return;
        }
    }

    public async Task CheckUpdates(bool userClick = false)
    {
        Logger.Information("Checking updates...");
        Client.DefaultRequestHeaders.Add("User-Agent", "MuseDashModToolsUI");

        var currentVersion = FileVersionInfo.GetVersionInfo(Environment.ProcessPath!).ProductVersion;
        Logger.Information("Get current version success: {Version}", currentVersion);
        try
        {
            var releases = await Client.GetFromJsonAsync<List<GithubRelease>>(ReleaseInfoLink);
            Logger.Information("Get releases success");

            var release = SavingService.Settings.DownloadPrerelease ? releases[0] : releases.Find(x => !x.Prerelease)!;

            var version = GetVersionFromTag(release.TagName);
            if (version is null || await SkipVersionCheck(version, currentVersion!, userClick)) return;

            var title = release.Name;
            var body = release.Body;
            if (!await UpdateRequired(release.TagName, title, body)) return;

            var link = GetDownloadLink(release.Assets);
            await LaunchUpdater(link);
            Logger.Information("Launch updater success, exit...");
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Check updates failed");
            await MessageBoxService.CreateErrorMessageBox(MsgBox_Content_CheckUpdateFailed.Localize());
        }
    }

    private async Task<List<Mod>?> GetModListFromSourceAsync(string downloadSource)
    {
        var url = downloadSource + "ModLinks.json";
        try
        {
            var mods = await Client.GetFromJsonAsync<List<Mod>>(url);
            Logger.Information("Get mod list from {Url} success", url);
            return mods;
        }
        catch
        {
            Logger.Warning("Get mod list from {Url} failed", url);
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

    private async Task<HttpResponseMessage?> DownloadMelonLoaderFromSource(string downloadSource, string path, IProgress<double> downloadProgress)
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

    private async Task LaunchUpdater(string link)
    {
        var currentDirectory = Directory.GetCurrentDirectory();
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

        Process.Start(updaterTargetPath, new[] { link, currentDirectory });
    }

    #region CheckUpdates Private Methods

    private SemanticVersion? GetVersionFromTag(string tagName)
    {
        var tag = VersionRegex().Match(tagName);
        if (!tag.Success) return null;
        if (!SemanticVersion.TryParse(tag.Groups[1].Value, out var version)) return null;
        Logger.Information("Get latest version success: {Version}", version);
        return version;
    }

    private async Task<bool> SkipVersionCheck(SemanticVersion version, string currentVersion, bool userClick)
    {
        var current = SemanticVersion.Parse(currentVersion)!;
        if (!userClick) return version == SavingService.Settings.SkipVersion || version <= current;
        if (version > current) return false;
        await MessageBoxService.CreateSuccessMessageBox(MsgBox_Content_LatestVersion.Localize());
        return true;
    }

    private async Task<bool> UpdateRequired(string version, string title, string body)
    {
        var update = await MessageBoxService.CreateCustomMarkDownConfirmMessageBox(
            string.Format(MsgBox_Content_NewerVersion.Localize(), version, title, body), 3);

        if (update == MsgBox_Button_NoNoAsk)
        {
            SavingService.Settings.SkipVersion = SemanticVersion.Parse(version);
            return false;
        }

        return update != MsgBox_Button_No;
    }

    private string GetDownloadLink(List<GitHubReleaseAsset> assets)
    {
        var osString = OperatingSystem.IsWindows() ? "Windows" : "Linux";

        var release = assets.Find(x => x.Name.Contains(osString, StringComparison.OrdinalIgnoreCase))!;
        var link = release.BrowserDownloadUrl;

        Logger.Information("Get {OsString} download link success {Link}", osString, link);
        return link;
    }

    [GeneratedRegex(@"v?(\d+\.\d+\.\d+-?\w*)")]
    private static partial Regex VersionRegex();

    #endregion
}