using Euterpe.Models.Charts.CustomAlbums;

namespace Euterpe.Abstractions;

public interface IChartLocalService
{
    IEnumerable<string> GetChartFolderPaths(ChartSource source);
    CustomAlbumSource CreateCustomAlbumSource(string path);
    CustomAlbumSource[] GetCustomAlbumsSources();
    Task<ChartDto?> LoadChartFromPathAsync(string chartFolder, ChartSource source);
}
