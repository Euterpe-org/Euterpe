namespace MuseDashModTools.Abstractions;

public interface ILocalService
{
    Task CheckDotNetRuntimeInstallAsync();
    Task<string> GetMuseDashFolderAsync();
    IEnumerable<string> GetModFilePaths();
    IEnumerable<string> GetLibFileNames();
    Task<bool> InstallMelonLoaderAsync();
    Task<bool> UninstallMelonLoaderAsync();
    ModDto? LoadModFromPath(string filePath);
    void LaunchGame(bool isModded);
    ValueTask<string> ReadGameVersionAsync();
    bool ExtractZipFile(string zipPath, string extractPath);
}