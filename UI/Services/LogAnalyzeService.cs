using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public ILogger Logger { get; init; }
    public IMessageBoxService MessageBoxService { get; init; }
    public ISettingService SettingService { get; init; }
    private string LogPath { get; set; } = string.Empty;
    private string LogContent { get; set; } = string.Empty;
    private string[] LogContentArray { get; set; }
    private StringBuilder LogContentBuilder { get; set; } = new();

    public async Task<bool> CheckPirate()
    {
        if (!LogContent.Contains("ApplicationPath"))
        {
            await MessageBoxService.CreateErrorMessageBox(MsgBox_Content_NoApplicationPath);
            return true;
        }

        LogContentArray = LogContent.Split("\r\n", StringSplitOptions.RemoveEmptyEntries).Select(x => x[15..]).ToArray();
        var gamePath = string.Empty;

        foreach (var line in LogContentArray)
        {
            if (!line.Contains("ApplicationPath")) continue;
            gamePath = line[24..];
        }

        if (!gamePath.Contains(@"steamapps\common\Muse Dash"))
        {
            Logger.Information(@"Game path doesn't contain 'steamapps\common\Muse Dash'");
            await MessageBoxService.CreateErrorMessageBox(MsgBox_Content_GamePathError.Localize());
            return true;
        }

        var steamPath = gamePath[..^40];
        var vdfPath = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
        if (!File.Exists(vdfPath)) return true;

        var stream = File.OpenRead(vdfPath);
        var kv = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);
        var data = kv.Deserialize<Dictionary<int, SteamLibraryFolder>>(stream);

        if (data.Values.Any(value => value.Path?.Replace(@"\\", @"\") == steamPath && value.Apps.ContainsKey(774171)))
            return false;

        Logger.Information("Cannot found download record in libraryfolders.vdf");
        await MessageBoxService.CreateErrorMessageBox(MsgBox_Content_NoInstallRecord.Localize());
        return true;
    }

    public async Task<bool> CheckMelonLoaderVersion()
    {
        var melonLoaderVersion = LogContentArray.First(x => x.Contains("MelonLoader")).Substring(13, 5);
        if (melonLoaderVersion == "0.5.7") return true;

        Logger.Information("Incorrect MelonLoader Version: {MelonLoaderVersion}", melonLoaderVersion);
        await MessageBoxService.CreateErrorMessageBox(string.Format(MsgBox_Content_IncorrectMelonLoaderVersion.Localize(), melonLoaderVersion));
        return false;
    }

    public async Task<string> LoadLog()
    {
        if (!string.IsNullOrEmpty(SettingService.Settings.MelonLoaderFolder))
            LogPath = Path.Combine(SettingService.Settings.MelonLoaderFolder, "Latest.log");
        if (!File.Exists(LogPath)) return string.Empty;

        try
        {
            LogContent = await File.ReadAllTextAsync(LogPath);
            Logger.Information("Read Log Success");
            return LogContent;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Read Log Error");
            return string.Empty;
        }
    }
}