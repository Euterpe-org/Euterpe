namespace MuseDashModTools.Abstractions;

public interface ILocalService
{
    Task CheckDotNetRuntimeInstallAsync();
    Task<string> GetMuseDashFolderAsync();
    IEnumerable<string> GetModFilePaths();
    IEnumerable<string> GetLibFilePaths();
    Task<bool> InstallMelonLoaderAsync();
    Task<bool> UninstallMelonLoaderAsync();
    ModDto? LoadModFromPath(string filePath);
    LibDto LoadLibFromPath(string filePath);
    ValueTask<string> ReadGameVersionAsync();
    string? ReadMelonLoaderVersion();
    bool ExtractZipFile(string zipPath, string extractPath);
}