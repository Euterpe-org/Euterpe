namespace Euterpe.Abstractions;

public interface IChartManageService
{
    IObservable<IChangeSet<ChartDto, string>> Connect();
    Task InitializeChartsAsync();
    Task RefreshOfflineChartsAsync();
    Task DownloadChartAsync(string chartId, CancellationToken cancellationToken = default);
    Task UpdateChartAsync(string chartId, CancellationToken cancellationToken = default);
    Task RemoveChartAsync(string folderPath);
    Task UpdateAllChartsAsync(CancellationToken cancellationToken = default);
}