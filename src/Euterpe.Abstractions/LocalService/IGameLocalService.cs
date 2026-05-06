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
    ValueTask ReadGameInformationAsync();
    void ReadMelonLoaderVersion();
}