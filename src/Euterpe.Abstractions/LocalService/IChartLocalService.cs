namespace Euterpe.Abstractions;

public interface IChartLocalService
{
    string[] GetChartFolderPaths(ChartSource source);
    string[] GetCustomAlbumsChartFilePaths();
    Task<ChartDto?> LoadChartFromPathAsync(string chartFolder, ChartSource source);
}