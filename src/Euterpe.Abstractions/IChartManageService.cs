namespace Euterpe.Abstractions;

public interface IChartManageService
{
    // Stream + lifecycle
    IObservable<IChangeSet<ChartDto, string>> Connect();
    Task InitializeChartsAsync();

    // Single-chart operations
    Task DownloadChartAsync(string chartId, IProgress<BatchProgress>? progress = null, CancellationToken cancellationToken = default);
    Task UpdateChartAsync(string chartId, CancellationToken cancellationToken = default);
    Task RemoveChartAsync(string folderPath);
    Task RefreshChartAsync(string folderPath);

    // Bulk operations
    Task<int> UpdateAllChartsAsync(CancellationToken cancellationToken = default);
    Task<int> MigrateCustomAlbumsAsync(IProgress<BatchProgress>? progress = null, CancellationToken cancellationToken = default);
    Task<bool> ImportChartsAsync(IReadOnlyList<string> paths, CancellationToken cancellationToken = default);

    // Disk reconciliation
    Task ReconcileChartsAsync();
    Task ReconcileChartsAsync(IReadOnlySet<string> changedFolders);
}
