namespace Euterpe.Core;

internal sealed partial class ChartManageService
{
    private async Task LoadChartsCoreAsync()
    {
        await LoadFromSourceAsync(ChartSource.Offline).ConfigureAwait(false);
        await LoadFromSourceAsync(ChartSource.Online).ConfigureAwait(false);

        Logger.ZLogInformation($"All charts loaded");
    }

    private async Task DownloadChartCoreAsync(string chartId, CancellationToken cancellationToken)
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

    private async Task<ChartUpdateResult> UpdateChartCoreAsync(string cid, IReadOnlyCollection<string> changedFiles, CancellationToken cancellationToken)
    {
        try
        {
            var folderPath = await GameDownloadManager.UpdateChartAsync(cid, changedFiles, cancellationToken).ConfigureAwait(false);

            var chart = await ChartLocalService.LoadChartFromPathAsync(folderPath, ChartSource.Online).ConfigureAwait(false);
            if (chart is null)
            {
                Logger.ZLogWarning($"Updated chart {cid} but failed to load it from {folderPath}");
                return new ChartUpdateResult(false, cid);
            }

            _sourceCache.AddOrUpdate(chart);
            Logger.ZLogInformation($"Chart {cid} updated");
            return new ChartUpdateResult(true, chart.Manifest.Meta.Name);
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to update chart {cid}");
            return new ChartUpdateResult(false, cid);
        }
    }

    private Task RemoveChartCoreAsync(string folderPath)
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

    private Task RemoveDelistedChartCoreAsync(string cid)
    {
        var chart = GetOnlineCharts().FirstOrDefault(c => c.FolderName == cid);
        if (chart is null)
        {
            return Task.CompletedTask;
        }

        if (FileSystemService.TryDeleteDirectory(chart.FolderPath, DeleteOption.IgnoreIfNotFound))
        {
            _sourceCache.RemoveKey(chart.FolderPath);
            Logger.ZLogInformation($"Removed delisted chart {cid}");
        }
        else
        {
            Logger.ZLogError($"Failed to remove delisted chart {cid}");
        }

        return Task.CompletedTask;
    }

    private Task RunExclusiveAsync(string key, Func<Task> action) => _singleFlight.RunAsync(key, action);

    private async Task<T> RunExclusiveAsync<T>(string key, Func<Task<T>> action)
    {
        T result = default!;
        await _singleFlight.RunAsync(key, async () => result = await action().ConfigureAwait(false)).ConfigureAwait(false);
        return result;
    }

    private readonly record struct ChartUpdateResult(bool Success, string DisplayName);
}