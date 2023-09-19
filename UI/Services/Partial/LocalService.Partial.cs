using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace MuseDashModToolsUI.Services;

public partial class LocalService
{
    /// <summary>
    ///     Verify game exe version
    /// </summary>
    /// <returns>Is correct version</returns>
    [SupportedOSPlatform(nameof(OSPlatform.Windows))]
    private async Task<bool> VerifyGameVersion()
    {
        var version = FileVersionInfo.GetVersionInfo(SavingService.Settings.MuseDashExePath).FileVersion;
        if (version is "2019.4.32.16288752") return true;
        Logger.Error("Incorrect game version {Version}, showing error message box...", version);
        await MessageBoxService.ErrorMessageBox(MsgBox_Content_IncorrectVersion);
        return false;
    }

    #region CheckValidPath Private Methods

    /// <summary>
    ///     Check Game Files Exist
    /// </summary>
    private async Task CheckGameFileExist()
    {
        var gameAssemblyPath = Path.Join(SavingService.Settings.MuseDashFolder, "GameAssembly.dll");

        if (!File.Exists(SavingService.Settings.MuseDashExePath) || !File.Exists(gameAssemblyPath))
        {
            Logger.Error("No game files found in {Path}, showing error message box...", SavingService.Settings.MuseDashFolder);
            await MessageBoxService.ErrorMessageBox(MsgBox_Content_NoExeFound);
            await SavingService.OnChoosePath();
            await CheckGameFileExist();
        }

        Logger.Information("Game files exists");
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

    #region LaunchUpdater Private Methods

    /// <summary>
    ///     Get Updater file path
    /// </summary>
    /// <param name="folder">Created Updater folder path</param>
    /// <returns>Updater file path</returns>
    private static string GetUpdaterFilePath(string folder)
    {
        if (OperatingSystem.IsWindows()) return Path.Combine(folder, "Updater.exe");
        if (OperatingSystem.IsLinux()) return Path.Combine(folder, "Updater");

        return Path.Combine(folder, "Updater.exe");
    }

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

    #endregion
}