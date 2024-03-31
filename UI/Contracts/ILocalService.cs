namespace MuseDashModToolsUI.Contracts;

public interface ILocalService
{
    /// <summary>
    ///     Check DotNet Runtime Install
    /// </summary>
    /// <returns></returns>
    Task CheckDotNetRuntimeInstallAsync();

    /// <summary>
    ///     Check MelonLoader Install
    /// </summary>
    Task CheckMelonLoaderInstallAsync();

    /// <summary>
    ///     Check whether the chosen path is valid
    /// </summary>
    Task CheckValidPathAsync();

    /// <summary>
    ///     Get Mod dll paths
    /// </summary>
    /// <param name="path"></param>
    /// <returns>Mod dll paths</returns>
    IEnumerable<string> GetModFiles(string path);

    /// <summary>
    ///     Get Bms files
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    string[] GetBmsFiles(string path);

    /// <summary>
    ///     Launch Updater
    /// </summary>
    /// <param name="link">Download link</param>
    /// <returns>Launch Success</returns>
    Task<bool> LaunchUpdaterAsync(string link);

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
    Task OnInstallMelonLoaderAsync();

    /// <summary>
    ///     Uninstall MelonLoader
    /// </summary>
    /// <returns></returns>
    Task OnUninstallMelonLoaderAsync();

    /// <summary>
    ///     Launch Game
    /// </summary>
    /// <param name="isModded"></param>
    void OnLaunchGame(bool isModded);

    /// <summary>
    ///     Open CustomAlbums folder
    /// </summary>
    /// <returns></returns>
    Task OpenCustomAlbumsFolderAsync();

    /// <summary>
    ///     Open Mods folder
    /// </summary>
    /// <returns></returns>
    Task OpenModsFolderAsync();

    /// <summary>
    ///     Open UserData folder
    /// </summary>
    /// <returns></returns>
    Task OpenUserDataFolderAsync();

    /// <summary>
    ///     Open Log folder
    /// </summary>
    /// <returns></returns>
    Task OpenLogFileAsync();

    /// <summary>
    ///     Read game version
    /// </summary>
    /// <returns>Game Version</returns>
    ValueTask<string> ReadGameVersionAsync();
}