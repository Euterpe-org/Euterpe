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

    public async Task UpdateChartAsync(string chartId, CancellationToken cancellationToken = default)
    {
        var chart = GetOnlineCharts().FirstOrDefault(c => c.FolderName == chartId);
        if (chart is null)
        {
            Logger.ZLogWarning($"Update requested for unknown online chart {chartId}");
            return;
        }

        foreach (var (success, displayName) in await CheckAndApplyUpdatesAsync([chart], cancellationToken).ConfigureAwait(false))
        {
            if (success)
            {
                NotificationService.SuccessLight(Notification_Content_Chart_Update_Success, displayName);
            }
            else
            {
                NotificationService.ErrorLight(Notification_Content_Chart_Update_Failed, displayName);
            }
        }
    }

    public Task RemoveChartAsync(string folderPath) =>
        RunExclusiveAsync(Path.GetFileName(folderPath), () => RemoveChartCoreAsync(folderPath));

    public async Task UpdateAllChartsAsync(CancellationToken cancellationToken = default)
    {
        var results = await CheckAndApplyUpdatesAsync(GetOnlineCharts(), cancellationToken).ConfigureAwait(false);
        var updated = results.Count(r => r.Success);
        var failed = results.Count - updated;

        if (failed > 0)
        {
            NotificationService.WarningLight(Notification_Content_Chart_UpdateAll_Partial, updated, failed);
        }
        else if (updated > 0)
        {
            NotificationService.SuccessLight(Notification_Content_Chart_UpdateAll_Success, updated);
        }
    }

    #region Injections

    public required IChartLocalService ChartLocalService { get; init; }
    public required IFileSystemService FileSystemService { get; init; }
    public required IGameDownloadManager GameDownloadManager { get; init; }
    public required ILogger<ChartManageService> Logger { get; init; }
    public required INotificationService NotificationService { get; init; }

    #endregion Injections
}
