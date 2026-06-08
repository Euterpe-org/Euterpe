using Euterpe.Shared.Threading;

namespace Euterpe.Core;

internal sealed partial class ChartManageService : IChartManageService
{
    private readonly Lazy<Task> _initTask;
    private readonly SingleFlight<string> _singleFlight = new();
    private readonly SourceCache<ChartDto, string> _sourceCache = new(x => x.FolderPath);

    public ChartManageService() => _initTask = new Lazy<Task>(LoadChartsCoreAsync, LazyThreadSafetyMode.ExecutionAndPublication);

    public IObservable<IChangeSet<ChartDto, string>> Connect() => _sourceCache.Connect();

    public Task InitializeChartsAsync() => _initTask.Value;

    public Task RefreshOfflineChartsAsync() => LoadFromSourceAsync(ChartSource.Offline);

    public Task DownloadChartAsync(string chartId, CancellationToken cancellationToken = default) =>
        RunExclusiveAsync(chartId, () => DownloadChartCoreAsync(chartId, cancellationToken));

    public Task UpdateChartAsync(string chartId, CancellationToken cancellationToken = default)
    {
        var chart = GetOnlineCharts().FirstOrDefault(c => c.FolderName == chartId);
        if (chart is not null)
        {
            return CheckAndApplyUpdatesAsync([chart], cancellationToken);
        }

        Logger.ZLogWarning($"Update requested for unknown online chart {chartId}");
        return Task.CompletedTask;
    }

    public Task RemoveChartAsync(string folderPath) =>
        RunExclusiveAsync(Path.GetFileName(folderPath), () => RemoveChartCoreAsync(folderPath));

    public Task UpdateAllChartsAsync(CancellationToken cancellationToken = default) =>
        CheckAndApplyUpdatesAsync(GetOnlineCharts(), cancellationToken);

    #region Injections

    public required IChartLocalService ChartLocalService { get; init; }
    public required IFileSystemService FileSystemService { get; init; }
    public required IGameDownloadManager GameDownloadManager { get; init; }
    public required ILogger<ChartManageService> Logger { get; init; }
    public required INotificationService NotificationService { get; init; }

    #endregion Injections
}