using Euterpe.Shared.Threading;

namespace Euterpe.Core;

internal sealed partial class ChartManageService : IChartManageService, IDisposable
{
    private readonly Lazy<Task> _initTask;
    private readonly SingleFlight<string> _singleFlight = new();
    private readonly SourceCache<ChartDto, string> _sourceCache = new(x => x.FolderPath);

    public ChartManageService() => _initTask = new Lazy<Task>(InitializeCoreAsync, LazyThreadSafetyMode.ExecutionAndPublication);

    public IObservable<IChangeSet<ChartDto, string>> Connect() => _sourceCache.Connect();

    public Task InitializeChartsAsync() => _initTask.Value;

    public Task DownloadChartAsync(string cid, IProgress<BatchProgress>? progress = null, CancellationToken cancellationToken = default) =>
        FindOnlineChartByCid(cid) is null
            ? RunExclusiveAsync(cid, () => DownloadChartCoreAsync(cid, progress, cancellationToken))
            : UpdateChartAsync(cid, cancellationToken);

    public async Task UpdateChartAsync(string cid, CancellationToken cancellationToken = default)
    {
        if (FindOnlineChartByCid(cid) is not { } chart)
        {
            Logger.ZLogWarning($"Update requested for unknown online chart {cid}");
            NotificationService.ErrorLight(Notification_Content_Chart_Update_Failed, cid);
            return;
        }

        var results = await CheckAndApplyUpdatesAsync([chart], cancellationToken).ConfigureAwait(false);
        if (results is not [var (success, displayName)])
        {
            NotificationService.NoticeLight(Notification_Content_Chart_Update_UpToDate, chart.Manifest.Meta.Name);
            return;
        }

        if (success)
        {
            NotificationService.SuccessLight(Notification_Content_Chart_Update_Success, displayName);
        }
        else
        {
            NotificationService.ErrorLight(Notification_Content_Chart_Update_Failed, displayName);
        }
    }

    public Task RemoveChartAsync(string folderPath) =>
        RunExclusiveAsync(Path.GetFileName(folderPath), () => RemoveChartCoreAsync(folderPath));

    public Task RefreshChartAsync(string folderPath) =>
        RunExclusiveAsync(Path.GetFileName(folderPath), () => RefreshChartCoreAsync(folderPath));

    public async Task<int> UpdateAllChartsAsync(CancellationToken cancellationToken = default)
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

        return results.Count;
    }

    public async Task<int> DeleteChartsAsync(IReadOnlyList<string> folderPaths, CancellationToken cancellationToken = default)
    {
        var succeeded = 0;

        foreach (var folderPath in folderPaths)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (await RunExclusiveAsync(Path.GetFileName(folderPath), () => Task.FromResult(RemoveLocalChart(folderPath))).ConfigureAwait(false))
            {
                succeeded++;
            }
        }

        var failed = folderPaths.Count - succeeded;
        if (failed > 0)
        {
            NotificationService.WarningLight(Notification_Content_Chart_BulkDelete_Partial, succeeded, failed);
        }
        else if (succeeded > 0)
        {
            NotificationService.SuccessLight(Notification_Content_Chart_BulkDelete_Success, succeeded);
        }

        return succeeded;
    }

    #region Injections

    public required GameConfig GameConfig { get; init; }
    public required IArchiveService Archive { get; init; }
    public required IChartLocalService ChartLocalService { get; init; }
    public required IFileSystemService FileSystemService { get; init; }
    public required IGameDownloadManager GameDownloadManager { get; init; }
    public required ILogger<ChartManageService> Logger { get; init; }
    public required INotificationService NotificationService { get; init; }
    public required IMigrationService MigrationService { get; init; }

    #endregion Injections
}
