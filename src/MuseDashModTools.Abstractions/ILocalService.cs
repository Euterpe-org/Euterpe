namespace MuseDashModTools.Abstractions;

public interface ILocalService
{
    Task<bool> CheckDotNetRuntimeInstalledAsync();
    Task<bool> CheckDotNetSdkInstalledAsync();
    Task<bool> CheckModTemplateInstalledAsync();
    Task<string> GetMuseDashFolderAsync();
    string[] GetModFilePaths();
    string[] GetLibFilePaths();
    Task<bool> InstallMelonLoaderAsync();
    Task<bool> UninstallMelonLoaderAsync();
    Task<ModDto?> LoadModFromPathAsync(string filePath);
    Task<LibDto> LoadLibFromPathAsync(string filePath);
    ValueTask<string> ReadGameVersionAsync();
    string? ReadMelonLoaderVersion();
    bool ExtractZipFile(string zipPath, string extractPath);
}