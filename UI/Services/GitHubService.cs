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
    public HttpClient Client { get; init; }
    public IMessageBoxService MessageBoxService { get; init; }
    public ILogger Logger { get; init; }

    public async Task<List<Mod>> GetModsAsync()
    {
        List<Mod> mods;
        try
        {
            mods = (await Client.GetFromJsonAsync<List<Mod>>(PrimaryLink + "ModLinks.json"))!;
            Logger.Information("Get mods list from primary link success");
        }
        catch
        {
            Logger.Warning("Get mods list from primary link failed, try secondary link...");
            try
            {
                mods = (await Client.GetFromJsonAsync<List<Mod>>(SecondaryLink + "ModLinks.json"))!;
                Logger.Information("Get mods list from secondary link success");
            }
            catch
            {
                Logger.Warning("Get mods list from secondary link failed, try third link...");
                mods = (await Client.GetFromJsonAsync<List<Mod>>(ThirdLink + "ModLinks.json"))!;
                Logger.Information("Get mods list from third link success");
            }
        }

        return mods;
    }

    public async Task DownloadModAsync(string link, string path)
    {
        HttpResponseMessage result;
        try
        {
            result = await Client.GetAsync(PrimaryLink + link);
            Logger.Information("Download mod from primary link success");
        }
        catch
        {
            try
            {
                Logger.Warning("Download mod from primary link failed, try secondary link...");
                result = await Client.GetAsync(SecondaryLink + link);
                Logger.Information("Download mod from secondary link success");
            }
            catch
            {
                Logger.Warning("Download mod from secondary link failed, try third link...");
                result = await Client.GetAsync(ThirdLink + link);
                Logger.Information("Download mod from third link success");
            }
        }

        await using var fs = new FileStream(path, FileMode.OpenOrCreate);
        await result.Content.CopyToAsync(fs);
    }

    public async Task DownloadMelonLoader(string path, IProgress<double> downloadProgress)
    {
        HttpResponseMessage result;
        try
        {
            result = await Client.GetAsync(PrimaryLink + "MelonLoader.zip", HttpCompletionOption.ResponseHeadersRead);
            Logger.Information("Get MelonLoader Download ResponseHeader from primary link success");
        }
        catch
        {
            try
            {
                Logger.Warning("Get MelonLoader Download ResponseHeader from primary link failed, try secondary link...");
                result = await Client.GetAsync(SecondaryLink + "MelonLoader.zip", HttpCompletionOption.ResponseHeadersRead);
                Logger.Information("Get MelonLoader Download ResponseHeader from secondary link success");
            }
            catch
            {
                Logger.Warning("Get MelonLoader Download ResponseHeader from secondary link failed, try third link...");
                result = await Client.GetAsync(ThirdLink + "MelonLoader.zip", HttpCompletionOption.ResponseHeadersRead);
                Logger.Information("Get MelonLoader Download ResponseHeader from third link success");
            }
        }

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

            if (!doc.RootElement.TryGetProperty("tag_name", out var tagName))
                return;

            var tag = tagName.GetString();
            if (tag is null) return;
            if (!Version.TryParse(tag.StartsWith('v') ? tag[1..] : tag, out var version)) return;
            Logger.Information("Get latest version success: {Version}", version);
            if (version <= currentVersion)
            {
                if (userClick)
                    await MessageBoxService.CreateMessageBox(MsgBox_Title_Success, MsgBox_Content_LatestVersion.Localize());
                return;
            }

            var title = doc.RootElement.GetProperty("name").GetString();
            var body = doc.RootElement.GetProperty("body").GetString();
            var update = await MessageBoxService.CreateConfirmMessageBox(MsgBox_Title_Notice,
                string.Format(MsgBox_Content_NewerVersion.Localize(), version, title, body));

            if (!update) return;
            var link = string.Empty;
            var assets = doc.RootElement.GetProperty("assets");
            var releases = assets.EnumerateArray();

            if (OperatingSystem.IsLinux())
            {
                var release = releases.FirstOrDefault(x =>
                    x.GetProperty("name").GetString()!.Contains("Linux", StringComparison.OrdinalIgnoreCase));
                link = release.GetProperty("browser_download_url").GetString()!;
                Logger.Information("Get Linux download link success{Link}", link);
            }

            if (OperatingSystem.IsWindows())
            {
                var release = releases.FirstOrDefault(x =>
                    x.GetProperty("name").GetString()!.Contains("Windows", StringComparison.OrdinalIgnoreCase));
                link = release.GetProperty("browser_download_url").GetString()!;
                Logger.Information("Get Windows download link success{Link}", link);
            }

            var currentDirectory = Directory.GetCurrentDirectory();
            await LaunchUpdater(currentDirectory, new[] { link, currentDirectory + ".zip", currentDirectory });
            Logger.Information("Launch updater success, exit...");
            Environment.Exit(0);
        }
        catch
        {
            await MessageBoxService.CreateErrorMessageBox(MsgBox_Content_CheckUpdateFailed.Localize());
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
            await MessageBoxService.CreateErrorMessageBox(string.Format(MsgBox_Content_CopyUpdaterFailed.Localize(), ex));
        }

        Process.Start(updaterTargetPath, launchArgs);
    }
}