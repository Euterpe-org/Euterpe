using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MsBox.Avalonia.Enums;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Extensions;
using MuseDashModToolsUI.Models;
using Serilog;
using ValveKeyValue;
using static MuseDashModToolsUI.Localization.Resources;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace MuseDashModToolsUI.Services;

public class LogAnalyzeService : ILogAnalyzeService
{
    private readonly FileSystemWatcher _watcher = new();
    public ILogger Logger { get; init; }
    public IMessageBoxService MessageBoxService { get; init; }
    public IModService ModService { get; init; }
    public ISettingService SettingService { get; init; }
    private string LogPath { get; set; } = string.Empty;
    private string LogContent { get; set; } = string.Empty;
    private string[] LogContentArray { get; set; }
    private StringBuilder LogContentBuilder { get; } = new();

    public async Task AnalyzeLog()
    {
        CheckModVersion();
        if (LogContentBuilder.Length > 0)
        {
            await MessageBoxService.CreateSuccessMessageBox(LogContentBuilder.ToString(), Icon.Warning);
            LogContentBuilder.Clear();
        }
        else
        {
            await MessageBoxService.CreateSuccessMessageBox("Log Analysis Completed");
            Logger.Information("Log Analysis Completed");
        }
    }

    public async Task<bool> CheckPirate()
    {
        if (!LogContent.Contains("ApplicationPath"))
        {
            await MessageBoxService.CreateSuccessMessageBox(MsgBox_Content_NoApplicationPath, Icon.Warning);
            return true;
        }

        LogContentArray = LogContent.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
        var gamePath = string.Empty;

        foreach (var line in LogContentArray)
        {
            if (!line.Contains("ApplicationPath")) continue;
            gamePath = line[39..];
        }

        if (!gamePath.Contains(@"steamapps\common\Muse Dash"))
        {
            Logger.Information(@"Game path doesn't contain 'steamapps\common\Muse Dash'");
            await MessageBoxService.CreateSuccessMessageBox(MsgBox_Content_GamePathError.Localize(), Icon.Warning);
            return true;
        }

        var steamPath = gamePath[..^40];
        var acfPath = Path.Combine(steamPath, "steamapps", "appmanifest_774171.acf");
        if (!File.Exists(acfPath))
        {
            await MessageBoxService.CreateSuccessMessageBox(MsgBox_Content_NoInstallRecord.Localize(), Icon.Warning);
            Logger.Information("Cannot find appmanifest_774171.acf");
            return true;
        }

        var stream = File.OpenRead(acfPath);
        var kv = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);
        var data = kv.Deserialize<AppState>(stream);

        if (data.Appid != 774171 || data.Name != "Muse Dash" || data.InstalledDepots.Keys.All(x => x != 774172))
        {
            await MessageBoxService.CreateSuccessMessageBox(MsgBox_Content_NoInstallRecord.Localize(), Icon.Warning);
            Logger.Information("Cannot find Muse Dash download record in file");
            return true;
        }

        if (data.InstalledDepots.Keys.Any(x => x == 1055810) && data.InstalledDepots[1055810]["dlcappid"] == 1055810)
        {
            Logger.Information("Muse Dash DLC purchased");
            return false;
        }

        await MessageBoxService.CreateSuccessMessageBox(MsgBox_Content_NoDlcPurchased.Localize(), Icon.Warning);
        Logger.Information("Cannot find Muse Dash DLC purchase record in file");
        return false;
    }

    public async Task<bool> CheckMelonLoaderVersion()
    {
        var melonLoaderVersion = LogContentArray.First(x => x.Contains("MelonLoader")).Substring(28, 5);
        if (melonLoaderVersion == "0.5.7")
        {
            Logger.Information("Correct MelonLoader Version: {MelonLoaderVersion}", melonLoaderVersion);
            return true;
        }

        Logger.Information("Incorrect MelonLoader Version: {MelonLoaderVersion}", melonLoaderVersion);
        await MessageBoxService.CreateSuccessMessageBox(
            string.Format(MsgBox_Content_IncorrectMelonLoaderVersion.Localize(), melonLoaderVersion), Icon.Warning);
        return false;
    }

    public async Task<string> LoadLog()
    {
        if (!string.IsNullOrEmpty(SettingService.Settings.MelonLoaderFolder))
            LogPath = Path.Combine(SettingService.Settings.MelonLoaderFolder, "Latest.log");
        if (!File.Exists(LogPath)) return MsgBox_Content_NoLogFile.Localize();

        try
        {
            LogContent = await File.ReadAllTextAsync(LogPath);
            Logger.Information("Read Log Success");
            StartLogFileMonitor();
            return LogContent;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Read Log Error");
            return string.Empty;
        }
    }

    private void CheckModVersion()
    {
        for (var i = 0; i < LogContentArray.Length; i++)
        {
            if (LogContentArray[i].Contains("Assembly: "))
            {
                var splitData = LogContentArray[i - 2][15..].Split(" v");
                var modName = splitData[0];
                var modVersion = splitData[1];
                var version = ModService.CompareVersion(modName, modVersion);
                switch (version)
                {
                    case 0:
                        continue;
                    case -1:
                        LogContentBuilder.AppendFormat(MsgBox_Content_OutdatedMod, modName).AppendLine();
                        break;
                }
            }

            if (LogContentArray[i].Contains("Mods loaded.")) break;
        }
    }

    private void StartLogFileMonitor()
    {
        _watcher.Path = SettingService.Settings.MelonLoaderFolder;
        _watcher.Filter = "Latest.log";
        _watcher.Renamed += async (_, _) => await LoadLog();
        _watcher.Changed += async (_, _) => await LoadLog();
        _watcher.Created += async (_, _) => await LoadLog();
        _watcher.Deleted += async (_, _) => await LoadLog();
        _watcher.EnableRaisingEvents = true;
        Logger.Information("Log File Monitor Started");
    }
}