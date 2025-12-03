namespace MuseDashModTools.Abstractions;

public interface ILocalService
{
    Task<bool> CheckDotNetSdkInstalledAsync();
    Task<bool> CheckModTemplateInstalledAsync();
    Task<string> GetSteamFolderAsync();
    Task<string> GetSteamExecPathAsync();
    Task<string> GetMuseDashFolderAsync();
    Task<string> GetCacheFolderAsync();
    string[] GetModFilePaths();
    string[] GetLibFilePaths();
    Task<bool> InstallMelonLoaderAsync();
    Task<bool> UninstallMelonLoaderAsync();
    Task<ModDto?> LoadModFromPathAsync(string filePath);
    Task<LibDto> LoadLibFromPathAsync(string filePath);
    ValueTask ReadGameInformationAsync();
    void ReadMelonLoaderVersion();
}