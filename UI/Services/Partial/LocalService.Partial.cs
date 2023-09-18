using System.IO;
using static MuseDashModToolsUI.Localization.Resources;

namespace MuseDashModToolsUI.Services;

public partial class LocalService
{
    #region CheckValidPath Private Methods

    private async Task CheckGameFileExist()
    {
        var gameAssemblyPath = Path.Join(SavingService.Settings.MuseDashFolder, "GameAssembly.dll");

        if (!File.Exists(SavingService.Settings.MuseDashExePath) || !File.Exists(gameAssemblyPath))
        {
            Logger.Error("No game files found in {Path}, showing error message box...", SavingService.Settings.MuseDashFolder);
            await MessageBoxService.CreateErrorMessageBox(MsgBox_Content_NoExeFound);
            await SavingService.OnChoosePath();
            await CheckGameFileExist();
        }

        Logger.Information("Game files exists");
    }

    private async Task CreateFiles()
    {
        if (!Directory.Exists(SavingService.Settings.ModsFolder))
        {
            Directory.CreateDirectory(SavingService.Settings.ModsFolder);
            Logger.Information("Mods folder not found, created");
        }

        if (!Directory.Exists(SavingService.Settings.UserDataFolder))
        {
            Directory.CreateDirectory(SavingService.Settings.UserDataFolder);
            Logger.Information("UserData folder not found, created");
        }

        var cfgFilePath = Path.Join(SavingService.Settings.MuseDashFolder, "UserData", "MuseDashModTools.cfg");
        await File.WriteAllTextAsync(cfgFilePath, Environment.ProcessPath);
        Logger.Information("Write path into cfg file successfully");
    }

    #endregion
}