using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Models;

namespace MuseDashModToolsUI.Services;

public class GitHubService : IGitHubService
{
    private readonly HttpClient _client;
    private readonly IDialogueService _dialogueService;

    private const string BaseLink = "MDModsDev/ModLinks/dev/";
    private const string ReleaseInfoLink = "https://api.github.com/repos/MDModsDev/MuseDashModToolsUI/releases/latest";

    private const string PrimaryLink = "https://raw.githubusercontent.com/";
    private const string SecondaryLink = "https://raw.fastgit.org/";

    public GitHubService(HttpClient client, IDialogueService dialogueService)
    {
        _client = client;
        _dialogueService = dialogueService;
    }

    public async Task<List<Mod>> GetModsAsync()
    {
        List<Mod> mods;
        try
        {
            mods = (await _client.GetFromJsonAsync<List<Mod>>(
                PrimaryLink + BaseLink +
                "ModLinks.json"))!;
        }
        catch (Exception)
        {
            mods = (await _client.GetFromJsonAsync<List<Mod>>(
                SecondaryLink + BaseLink +
                "ModLinks.json"))!;
        }

        return mods;
    }

    public async Task DownloadModAsync(string link, string path)
    {
        HttpResponseMessage result;
        try
        {
            result = await _client.GetAsync(PrimaryLink + BaseLink + link);
        }
        catch (Exception)
        {
            result = await _client.GetAsync(SecondaryLink + BaseLink + link);
        }

        await using var fs = new FileStream(path, FileMode.OpenOrCreate);
        await result.Content.CopyToAsync(fs);
    }

    public async Task DownloadMelonLoader(string path, IProgress<double> downloadProgress)
    {
        HttpResponseMessage result;
        try
        {
            result = await _client.GetAsync(PrimaryLink + BaseLink + "MelonLoader.zip", HttpCompletionOption.ResponseHeadersRead);
        }
        catch (Exception)
        {
            result = await _client.GetAsync(SecondaryLink + BaseLink + "MelonLoader.zip", HttpCompletionOption.ResponseHeadersRead);
        }

        var totalLength = result.Content.Headers.ContentLength;
        var contentStream = await result.Content.ReadAsStreamAsync();
        await using var fs = new FileStream(path, FileMode.OpenOrCreate);
        var buffer = new byte[5 * 1024];
        var readLength = 0L;
        int length;
        while ((length = await contentStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
        {
            readLength += length;
            if (totalLength > 0)
            {
                downloadProgress.Report(Math.Round((double)readLength / totalLength.Value * 100, 2));
            }

            fs.Write(buffer, 0, length);
        }
    }

    public async Task CheckUpdates()
    {
        _client.DefaultRequestHeaders.Add("User-Agent", "MuseDashModToolsUI");

        var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
        try
        {
            var result = await _client.GetStringAsync(ReleaseInfoLink);
            var doc = JsonDocument.Parse(result);

            if (!doc.RootElement.TryGetProperty("tag_name", out var tagName))
                return;

            var tag = tagName.GetString();
            if (tag is null) return;
            if (!Version.TryParse(tag, out var version)) return;
            if (version <= currentVersion) return;

            var link = string.Empty;
            var assets = doc.RootElement.GetProperty("assets");
            var releases = assets.EnumerateArray();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var release = releases.FirstOrDefault(x => x.GetProperty("name").GetString()!.EndsWith("Linux"));
                link = release.GetProperty("browser_download_url").GetString()!;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var release = releases.FirstOrDefault(x => x.GetProperty("name").GetString()!.EndsWith("Windows"));
                link = release.GetProperty("browser_download_url").GetString()!;
            }

            var currentDirectory = Directory.GetCurrentDirectory();
            var parentPath = new DirectoryInfo(currentDirectory).Parent!.FullName;
            await LaunchUpdater(currentDirectory, new[] { link, currentDirectory + ".zip", parentPath });
        }
        catch (Exception)
        {
            await _dialogueService.CreateErrorMessageBox("Checking updates failed\nIf you are in China Mainland please open VPN");
        }
    }

    private async Task LaunchUpdater(string currentDirectory, IEnumerable<string> launchArgs)
    {
        var updaterExePath = Path.Combine(currentDirectory, "Updater.exe");
        var updaterTargetFolder = Path.Combine(currentDirectory, "Update");
        var updaterTargetPath = Path.Combine(currentDirectory, "Update", "Updater.exe");
        if (!File.Exists(updaterExePath))
        {
            await _dialogueService.CreateErrorMessageBox("Cannot find Updater.exe\nPlease make sure you have downloaded full software");
            return;
        }

        if (!Directory.Exists(updaterTargetFolder))
        {
            Directory.CreateDirectory(updaterTargetFolder);
        }

        try
        {
            File.Copy(updaterExePath, updaterTargetPath, true);
        }
        catch (Exception ex)
        {
            await _dialogueService.CreateErrorMessageBox($"Cannot copy Updater.exe to target path\n{ex}");
        }

        Process.Start(updaterTargetPath, launchArgs);
    }
}