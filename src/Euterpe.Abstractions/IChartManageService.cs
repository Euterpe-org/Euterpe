namespace Euterpe.Abstractions;

public interface IChartManageService
{
    IObservable<IChangeSet<ChartDto, string>> Connect();
    Task InitializeChartsAsync();
    Task DownloadChartAsync(string chartId, CancellationToken cancellationToken = default);
    Task UpdateChartAsync(string chartId, CancellationToken cancellationToken = default);
    Task RemoveChartAsync(string folderPath);
    Task<int> UpdateAllChartsAsync(CancellationToken cancellationToken = default);
    Task<int> MigrateCustomAlbumsAsync(IProgress<string>? progress = null, CancellationToken cancellationToken = default);
    Task<bool> ImportChartsAsync(IReadOnlyList<string> paths, CancellationToken cancellationToken = default);
}
