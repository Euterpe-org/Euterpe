namespace MuseDashModTools.Contracts;

public interface ILocalService
{
    Task CheckDotNetRuntimeInstallAsync();
    Task<string> GetMuseDashFolderAsync();
    IEnumerable<string> GetModFilePaths();
    HashSet<string?> GetLibFileNames();
    Task<bool> InstallMelonLoaderAsync();
    Task<bool> UninstallMelonLoaderAsync();
    ModDto? LoadModFromPath(string filePath);
    void LaunchGame(bool isModded);
    ValueTask<string> ReadGameVersionAsync();
    bool ExtractZipFile(string zipPath, string extractPath);
}