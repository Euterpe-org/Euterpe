namespace Euterpe.Core;

internal sealed partial class ChartManageService : IChartManageService
{
    private readonly Lazy<Task> _initTask;
    private readonly SourceCache<ChartDto, string> _sourceCache = new(x => x.FolderPath);

    public ChartManageService() => _initTask = new Lazy<Task>(LoadChartsAsync, LazyThreadSafetyMode.ExecutionAndPublication);

    public IObservable<IChangeSet<ChartDto, string>> Connect() => _sourceCache.Connect();

    public Task InitializeChartsAsync() => _initTask.Value;

    public Task RefreshOfflineChartsAsync() => LoadFromSourceAsync(ChartSource.Offline);

    public async Task DownloadChartAsync(string chartId, CancellationToken cancellationToken = default)
    {
        try
        {
            var folderPath = await GameDownloadManager.DownloadChartAsync(chartId, cancellationToken).ConfigureAwait(false);

            var chart = await ChartLocalService.LoadChartFromPathAsync(folderPath, ChartSource.Online).ConfigureAwait(false);
            if (chart is null)
            {
                Logger.ZLogWarning($"Downloaded chart {chartId} but failed to load it from {folderPath}");
                NotificationService.ErrorLight(Notification_Content_Chart_Download_Failed, chartId);
                return;
            }

            _sourceCache.AddOrUpdate(chart);
            Logger.ZLogInformation($"Chart {chartId} downloaded and added to cache");
            NotificationService.SuccessLight(Notification_Content_Chart_Download_Success, chart.Manifest.Meta.Name);
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to download chart {chartId}");
            NotificationService.ErrorLight(Notification_Content_Chart_Download_Failed, chartId);
        }
    }

    public Task UpdateChartAsync(string chartId, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task RemoveChartAsync(string folderPath)
    {
        Logger.ZLogInformation($"Removing chart at {folderPath}");

        var name = _sourceCache.Lookup(folderPath) is { HasValue: true, Value: var chart }
            ? chart.Manifest.Meta.Name
            : Path.GetFileName(folderPath);

        if (FileSystemService.TryDeleteDirectory(folderPath, DeleteOption.IgnoreIfNotFound))
        {
            _sourceCache.RemoveKey(folderPath);
            Logger.ZLogInformation($"Chart at {folderPath} removed");
            NotificationService.SuccessLight(Notification_Content_Chart_Remove_Success, name);
        }
        else
        {
            Logger.ZLogError($"Failed to remove chart at {folderPath}");
            NotificationService.ErrorLight(Notification_Content_Chart_Remove_Failed, name);
        }

        return Task.CompletedTask;
    }

    public Task UpdateAllChartsAsync(CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    #region Injections

    public required IChartLocalService ChartLocalService { get; init; }
    public required IFileSystemService FileSystemService { get; init; }
    public required IGameDownloadManager GameDownloadManager { get; init; }
    public required ILogger<ChartManageService> Logger { get; init; }
    public required INotificationService NotificationService { get; init; }

    #endregion Injections
}