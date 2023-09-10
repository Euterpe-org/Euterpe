using MuseDashModToolsUI.Models;

namespace MuseDashModToolsUI.Contracts;

public interface ILocalService
{
    IEnumerable<string> GetModFiles(string path);
    Mod? LoadMod(string filePath);
    Task CheckValidPath();
    Task<string> ReadGameVersion();
    Task CheckMelonLoaderInstall();
    bool GetPathFromRegistry(out string folderPath);
    Task OnInstallMelonLoader();
    Task OnUninstallMelonLoader();
    Task OpenModsFolder();
    Task OpenUserDataFolder();
    Task OpenLogFolder();
}