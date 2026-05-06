namespace Euterpe.Abstractions;

public interface IGameLocalService
{
    Task<string> GetGameFolderAsync();
    string[] GetModFilePaths();
    string[] GetLibFilePaths();
    Task InstallMelonLoaderAsync();
    Task UninstallMelonLoaderAsync();
    Task<ModDto?> LoadModFromPathAsync(string filePath);
    Task<LibDto> LoadLibFromPathAsync(string filePath);
    void ReadGameInformation();
    void ReadMelonLoaderVersion();
}