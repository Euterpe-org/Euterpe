namespace Euterpe.Abstractions;

public interface IChartLocalService
{
    IEnumerable<string> GetChartFolderPaths(ChartSource source);
    string[] GetCustomAlbumsSourcePaths();
    Task<ChartDto?> LoadChartFromPathAsync(string chartFolder, ChartSource source);
}