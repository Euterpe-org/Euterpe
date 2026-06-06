namespace Euterpe.Abstractions;

public interface IGameLocalService
{
    Task<string> GetGameFolderAsync();
    string[] GetModFilePaths();
    string[] GetLibFilePaths();
    string[] GetChartFolderPaths();
    string[] GetCustomAlbumsChartFilePaths();
    Task InstallMelonLoaderAsync();
    Task UninstallMelonLoaderAsync();
    Task<ModDto?> LoadModFromPathAsync(string filePath);
    Task<LibDto> LoadLibFromPathAsync(string filePath);
    Task<ChartDto?> LoadChartFromPathAsync(string filePath);
    void ReadGameInformation();
    void ReadMelonLoaderVersion();
}