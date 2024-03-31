namespace MuseDashModToolsUI.Services;

public sealed partial class LocalService
{
    /// <summary>
    ///     Check updater files exist and create updater target folder
    /// </summary>
    /// <param name="updaterFilePath"></param>
    /// <param name="updaterTargetFolder"></param>
    /// <returns>Is exist and create success</returns>
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

    private async Task<bool> ValidatePathAsync()
    {
        if (IsValidPath)
        {
            return true;
        }

        Logger.Error("Not valid path, showing error message box...");
        await MessageBoxService.ErrorMessageBox(MsgBox_Content_ChooseCorrectPath);
        return false;
    }

    #region CheckValidPath Private Methods

    /// <summary>
    ///     Check Game Files Exist
    /// </summary>
    private async Task CheckGameFileExist()
    {
        while (true)
        {
            var gameAssemblyPath = Path.Join(Settings.MuseDashFolder, "GameAssembly.dll");

            if (!File.Exists(Settings.MuseDashExePath) || !File.Exists(gameAssemblyPath))
            {
                Logger.Error("No game files found in {Path}, showing error message box...", Settings.MuseDashFolder);
                await MessageBoxService.ErrorMessageBox(MsgBox_Content_NoExeFound);
                await SavingService.Value.OnChooseGamePathAsync();
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
        if (!Directory.Exists(Settings.ModsFolder))
        {
            Directory.CreateDirectory(Settings.ModsFolder);
            Logger.Information("Mods folder not found, created");
        }

        if (!Directory.Exists(Settings.UserDataFolder))
        {
            Directory.CreateDirectory(Settings.UserDataFolder);
            Logger.Information("UserData folder not found, created");
        }

        var cfgFilePath = Path.Join(Settings.MuseDashFolder, "UserData", "MuseDashModTools.cfg");
        await File.WriteAllTextAsync(cfgFilePath, Environment.ProcessPath);
        Logger.Information("Write path into cfg file successfully");
    }

    #endregion
}