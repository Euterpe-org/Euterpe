using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using Avalonia.Media.Imaging;
using DialogHostAvalonia;
using NuGet.Versioning;

namespace MuseDashModToolsUI.Services;

public partial class GitHubService
{
    /// <summary>
    ///     Get Mod list from download source
    /// </summary>
    /// <param name="downloadSource"></param>
    /// <returns>Mod list</returns>
    private async Task<List<Mod>?> GetModListFromSourceAsync(string downloadSource)
    {
        var url = downloadSource + "ModLinks.json";
        try
        {
            var modLinks = await Client.GetStringAsync(url);
            Logger.Information("Get mod list from {Url} success", url);

            await File.WriteAllTextAsync(SavingService.Value.ModLinksPath, modLinks);
            var mods = SerializationService.DeserializeModList();
            return mods;
        }
        catch
        {
            Logger.Warning("Get mod list from {Url} failed", url);
            return null;
        }
    }

    /// <summary>
    ///     Download mod from download source
    /// </summary>
    /// <param name="downloadSource"></param>
    /// <param name="link"></param>
    /// <param name="path">File path</param>
    /// <returns></returns>
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

    /// <summary>
    ///     Download MelonLoader
    /// </summary>
    /// <param name="downloadProgress"></param>
    /// <returns>Is success</returns>
    private async Task<bool> DownloadMelonLoaderFromSourceAsync(IProgress<double> downloadProgress)
    {
        try
        {
            var totalLength = _melonLoaderResponseMessage!.Content.Headers.ContentLength;
            var contentStream = await _melonLoaderResponseMessage.Content.ReadAsStreamAsync();
            await using var fs = new FileStream(SavingService.Value.Settings.MelonLoaderZipPath, FileMode.OpenOrCreate);
            var buffer = new byte[5 * 1024];
            var readLength = 0L;
            int length;
            while ((length = await contentStream.ReadAsync(buffer.AsMemory(0, buffer.Length), CancellationToken.None)) != 0)
            {
                readLength += length;
                if (totalLength > 0)
                {
                    downloadProgress.Report(Math.Round((double)readLength / totalLength.Value * 100, 2));
                }

                fs.Write(buffer, 0, length);
            }

            Logger.Information("Download MelonLoader.zip success");
            return true;
        }
        catch (Exception ex)
        {
            if (ex is HttpRequestException)
            {
                Logger.Error(ex, "Download MelonLoader.zip failed");
                await MessageBoxService.ErrorMessageBox(MsgBox_Content_InstallMelonLoaderFailed_Internet, ex);
                DialogHost.GetDialogSession("DownloadWindowDialog")?.Close(false);
                return false;
            }

            Logger.Error(ex, "Download MelonLoader.zip failed");
            await MessageBoxService.ErrorMessageBox(MsgBox_Content_InstallMelonLoaderFailed, ex);
            DialogHost.GetDialogSession("DownloadWindowDialog")?.Close(false);
            return false;
        }
    }

    /// <summary>
    ///     Get chart covers
    /// </summary>
    /// <param name="charts"></param>
    private async Task GetChartCovers(IEnumerable<Chart>? charts)
    {
        var semaphore = new SemaphoreSlim(20);
        var tasks = charts?.Select(async chart =>
        {
            await semaphore.WaitAsync();

            try
            {
                var stream = await LoadCoverAsync(chart);
                chart.Cover = await Task.Run(() => Bitmap.DecodeToWidth(stream, 200));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Get chart cover failed");
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks!);
    }

    /// <summary>
    ///     Get chart cover memory stream
    /// </summary>
    /// <param name="chart"></param>
    /// <returns></returns>
    private async Task<MemoryStream> LoadCoverAsync(Chart chart)
    {
        byte[] coverBytes;
        var fileName = $"{chart.Id}-cover.png";
        var filePath = Path.Combine(SavingService.Value.ChartFolderPath, fileName);

        if (!File.Exists(filePath))
        {
            coverBytes = await Client.GetByteArrayAsync(string.Format(ChartCoverApi, chart.Id));
            await File.WriteAllBytesAsync(filePath, coverBytes);
        }
        else
        {
            coverBytes = await File.ReadAllBytesAsync(filePath);
        }

        return new MemoryStream(coverBytes);
    }

    /// <summary>
    ///     Get MelonLoader response message
    /// </summary>
    private async Task GetMelonLoaderResponseMessage()
    {
        await GetMelonLoaderResponseFromSource(DefaultDownloadSource);
        if (_melonLoaderResponseMessage is not null)
        {
            return;
        }

        foreach (var pair in DownloadSourceDictionary.Where(pair => pair.Key != SavingService.Value.Settings.DownloadSource))
        {
            await GetMelonLoaderResponseFromSource(pair.Value);
            if (_melonLoaderResponseMessage is not null)
            {
                return;
            }
        }
    }

    /// <summary>
    ///     Get MelonLoader Response message from download source
    /// </summary>
    /// <param name="downloadSource"></param>
    private async Task GetMelonLoaderResponseFromSource(string downloadSource)
    {
        var url = downloadSource + "MelonLoader.zip";
        try
        {
            _melonLoaderResponseMessage = await Client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            Logger.Information("Get MelonLoader Download ResponseHeader from {Url} success", url);
        }
        catch
        {
            _melonLoaderResponseMessage = null;
            Logger.Warning("Get MelonLoader Download ResponseHeader from {Url} failed", url);
        }
    }

    #region CheckUpdates Private Methods

    /// <summary>
    ///     Get version number from github's tag name
    /// </summary>
    /// <param name="tagName"></param>
    /// <returns>Version</returns>
    private SemanticVersion? GetVersionFromTag(string tagName)
    {
        var tag = VersionRegex().Match(tagName);
        if (!tag.Success)
        {
            return null;
        }

        if (!SemanticVersion.TryParse(tag.Groups[1].Value, out var version))
        {
            return null;
        }

        Logger.Information("Get latest version success: {Version}", version);
        return version;
    }

    /// <summary>
    ///     Skip version check if current version is the saved SkipVersion, newer or equal than latest version
    ///     If user click the button to manually check updates then show a message box
    /// </summary>
    /// <param name="version"></param>
    /// <param name="currentVersion"></param>
    /// <param name="isUserClick"></param>
    /// <returns>Is skip</returns>
    private async Task<bool> SkipVersionCheck(SemanticVersion version, string currentVersion, bool isUserClick)
    {
        var current = SemanticVersion.Parse(currentVersion);
        if (!isUserClick)
        {
            return version == SavingService.Value.Settings.SkipVersion || version <= current;
        }

        if (version > current)
        {
            return false;
        }

        await MessageBoxService.SuccessMessageBox(MsgBox_Content_LatestVersion);
        return true;
    }

    /// <summary>
    ///     Show message box to ask user if they want to update
    /// </summary>
    /// <param name="version"></param>
    /// <param name="title"></param>
    /// <param name="body"></param>
    /// <returns>Is update</returns>
    private async Task<bool> UpdateRequired(string version, string title, string body)
    {
        var update = await MessageBoxService.CustomMarkDownConfirmMessageBox(
            string.Format(MsgBox_Content_NewerVersion, version, title, body), 3);

        if (update == MsgBox_Button_NoNoAsk)
        {
            SavingService.Value.Settings.SkipVersion = SemanticVersion.Parse(version);
            return false;
        }

        return update != MsgBox_Button_No && !string.IsNullOrEmpty(update);
    }

    /// <summary>
    ///     Get download link on different platform
    /// </summary>
    /// <param name="assets"></param>
    /// <returns>Download link</returns>
    private string GetDownloadLink(List<GitHubReleaseAsset> assets)
    {
        var release = assets.Find(x => x.Name.Contains(PlatformService.OsString, StringComparison.OrdinalIgnoreCase))!;
        var link = release.BrowserDownloadUrl;

        Logger.Information("Get {OsString} download link success {Link}", PlatformService.OsString, link);
        return link;
    }

    [GeneratedRegex(@"v?(\d+\.\d+\.\d+-?\w*)")]
    private static partial Regex VersionRegex();

    #endregion
}