using System.Collections.Generic;
using System.Threading.Tasks;
using MuseDashModToolsUI.Models;

namespace MuseDashModToolsUI.Contracts;

public interface ILocalService
{
    IEnumerable<string> GetModFiles(string path);
    Mod? LoadMod(string filePath);
    Task<bool> CheckValidPath();
    Task<string> ReadGameVersion();
    Task CheckMelonLoaderInstall();
    Task OnInstallMelonLoader();
    Task OnUninstallMelonLoader();
    Task<bool> CheckPirate(string logContent);
    Task OpenModsFolder();
    Task OpenUserDataFolder();
    Task OpenLogFolder();
}