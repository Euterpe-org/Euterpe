namespace Euterpe.Abstractions;

public interface ILocalService
{
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