using System.IO;

namespace MuseDashModToolsUI.Services;

public partial class LocalService
{
    /// <summary>
    ///     Check updater files exist
    /// </summary>
    /// <param name="updaterFilePath"></param>
    /// <param name="updaterTargetFolder"></param>
    /// <returns>Is exist</returns>
    private async Task<bool> CheckUpdaterFilesExist(string updaterFilePath, string updaterTargetFolder)
    {
        if (!File.Exists(updaterFilePath))
        {
            Logger.Error("Updater not found");
            await MessageBoxService.ErrorMessageBox(MsgBox_Content_UpdaterNotFound);
            return false;
        }

        if (!Directory.Exists(updaterTargetFolder))
        {
            Directory.CreateDirectory(updaterTargetFolder);
            Logger.Information("Create Update target folder success");
        }

        return true;
    }

    #region CheckValidPath Private Methods

    /// <summary>
    ///     Check Game Files Exist
    /// </summary>
    private async Task CheckGameFileExist()
    {
        while (true)
        {
            var gameAssemblyPath = Path.Join(SavingService.Settings.MuseDashFolder, "GameAssembly.dll");

            if (!File.Exists(SavingService.Settings.MuseDashExePath) || !File.Exists(gameAssemblyPath))
            {
                Logger.Error("No game files found in {Path}, showing error message box...", SavingService.Settings.MuseDashFolder);
                await MessageBoxService.ErrorMessageBox(MsgBox_Content_NoExeFound);
                await SavingService.GetFolderPath();
            }
            else
            {
                Logger.Information("Game files exists");
                break;
            }
        }
    }

    /// <summary>
    ///     Create Mods and UserData folder, and write path into cfg file
    /// </summary>
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