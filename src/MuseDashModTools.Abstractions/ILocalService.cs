namespace MuseDashModTools.Abstractions;

public interface ILocalService
{
    Task CheckDotNetRuntimeInstallAsync();
    Task<string> GetMuseDashFolderAsync();
    IEnumerable<string> GetModFilePaths();
    HashSet<string?> GetLibFileNames();
    Task BrowseFolderAsync(string path, string? selectedFileName = null);
    Task<bool> InstallMelonLoaderAsync();
    Task<bool> UninstallMelonLoaderAsync();
    ModDto? LoadModFromPath(string filePath);
    void LaunchGame(bool isModded);
    ValueTask<string> ReadGameVersionAsync();
    bool ExtractZipFile(string zipPath, string extractPath);
}