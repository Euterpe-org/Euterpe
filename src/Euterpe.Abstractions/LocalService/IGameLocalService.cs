namespace Euterpe.Abstractions;

public interface IGameLocalService
{
    Task<string> GetGameFolderAsync();
    string[] GetChartFolderPaths();
    string[] GetCustomAlbumsChartFilePaths();
    Task InstallMelonLoaderAsync();
    Task UninstallMelonLoaderAsync();
    Task<ChartDto?> LoadChartFromPathAsync(string filePath);
    void ReadGameInformation();
    void ReadMelonLoaderVersion();
}