using MuseDashModToolsUI.Models;

namespace MuseDashModToolsUI.Contracts;

public interface ILocalService
{
    /// <summary>
    ///     Check MelonLoader Install
    /// </summary>
    Task CheckMelonLoaderInstall();

    /// <summary>
    ///     Check whether the chosen path is valid
    /// </summary>
    Task CheckValidPath();

    /// <summary>
    ///     Get Mod dll paths
    /// </summary>
    /// <param name="path"></param>
    /// <returns>Mod dll paths</returns>
    IEnumerable<string> GetModFiles(string path);

    /// <summary>
    ///     Get game path from registry
    /// </summary>
    /// <param name="folderPath">Game folder path</param>
    /// <returns>Success</returns>
    bool GetPathFromRegistry(out string folderPath);

    /// <summary>
    ///     Launch Updater
    /// </summary>
    /// <param name="link">Download link</param>
    /// <returns>Launch Success</returns>
    Task<bool> LaunchUpdater(string link);

    /// <summary>
    ///     Load Mod
    /// </summary>
    /// <param name="filePath">File path</param>
    /// <returns></returns>
    Mod? LoadMod(string filePath);

    /// <summary>
    ///     Install MelonLoader
    /// </summary>
    /// <returns></returns>
    Task OnInstallMelonLoader();

    /// <summary>
    ///     Uninstall MelonLoader
    /// </summary>
    /// <returns></returns>
    Task OnUninstallMelonLoader();

    /// <summary>
    ///     Open Mods folder
    /// </summary>
    /// <returns></returns>
    Task OpenModsFolder();

    /// <summary>
    ///     Open UserData folder
    /// </summary>
    /// <returns></returns>
    Task OpenUserDataFolder();

    /// <summary>
    ///     Open Log folder
    /// </summary>
    /// <returns></returns>
    Task OpenLogFolder();

    /// <summary>
    ///     Read game version
    /// </summary>
    /// <returns>Game Version</returns>
    Task<string> ReadGameVersion();
}