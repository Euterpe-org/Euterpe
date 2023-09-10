using MuseDashModToolsUI.Models;

namespace MuseDashModToolsUI.Contracts;

public interface ILocalService
{
    Task CheckMelonLoaderInstall();
    Task CheckValidPath();
    IEnumerable<string> GetModFiles(string path);
    bool GetPathFromRegistry(out string folderPath);
    Mod? LoadMod(string filePath);
    Task OnInstallMelonLoader();
    Task OnUninstallMelonLoader();
    Task OpenModsFolder();
    Task OpenUserDataFolder();
    Task OpenLogFolder();
    Task<string> ReadGameVersion();
}