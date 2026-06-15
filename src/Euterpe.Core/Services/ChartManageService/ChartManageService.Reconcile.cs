namespace Euterpe.Core;

internal sealed partial class ChartManageService
{
    public async Task ReconcileChartsAsync()
    {
        var diskFolders = EnumerateChartFolders().ToArray();
        var diskKeys = diskFolders.Select(static folder => folder.Path).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var missing = (await diskFolders
                .Where(folder => !_sourceCache.Lookup(folder.Path).HasValue)
                .WhenAllAsync(folder => ChartLocalService.LoadChartFromPathAsync(folder.Path, folder.Source)).ConfigureAwait(false))
            .OfType<ChartDto>()
            .ToArray();
        _sourceCache.AddOrUpdate(missing);

        foreach (var key in _sourceCache.Keys.ToArray())
        {
            if (!diskKeys.Contains(key) && !Directory.Exists(key))
            {
                _sourceCache.RemoveKey(key);
            }
        }

        Logger.ZLogInformation($"Reconciled charts: {_sourceCache.Count} present, {missing.Length} newly loaded");
    }

    public async Task ReconcileChartsAsync(IReadOnlySet<string> changedFolders)
    {
        var added = new List<string>();
        var removed = new List<string>();

        foreach (var folder in changedFolders)
        {
            if (ResolveChartSource(folder) is not { } source)
            {
                continue;
            }

            var existing = _sourceCache.Lookup(folder);
            if (Directory.Exists(folder))
            {
                if (!existing.HasValue && await LoadAndCacheChartAsync(folder, source).ConfigureAwait(false) is { } chart)
                {
                    added.Add(chart.Manifest.Meta.Name);
                }
            }
            else if (existing.HasValue)
            {
                _sourceCache.RemoveKey(folder);
                removed.Add(existing.Value.Manifest.Meta.Name);
            }
        }

        NotifyChartSync(added, removed);
    }

    private async Task<ChartDto?> LoadAndCacheChartAsync(string folder, ChartSource source)
    {
        var chart = await ChartLocalService.LoadChartFromPathAsync(folder, source).ConfigureAwait(false);
        if (chart is not null)
        {
            _sourceCache.AddOrUpdate(chart);
            Logger.ZLogInformation($"Reconciled chart at {folder}");
        }

        return chart;
    }

    private IEnumerable<(string Path, ChartSource Source)> EnumerateChartFolders() =>
        EnumerateSource(ChartSource.Offline).Concat(EnumerateSource(ChartSource.Online));

    private IEnumerable<(string Path, ChartSource Source)> EnumerateSource(ChartSource source) =>
        ChartLocalService.GetChartFolderPaths(source).Select(path => (path, source));

    private ChartSource? ResolveChartSource(string folder)
    {
        if (folder.StartsWith(GameConfig.OnlineChartsFolder, StringComparison.OrdinalIgnoreCase))
        {
            return ChartSource.Online;
        }

        if (folder.StartsWith(GameConfig.OfflineChartsFolder, StringComparison.OrdinalIgnoreCase))
        {
            return ChartSource.Offline;
        }

        return null;
    }

    private void NotifyChartSync(IReadOnlyList<string> added, IReadOnlyList<string> removed)
    {
        switch (added.Count, removed.Count)
        {
            case (0, 0):
                break;
            case (1, 0):
                NotificationService.SuccessLight(Notification_Content_Chart_Sync_Added, added[0]);
                break;
            case (0, 1):
                NotificationService.NoticeLight(Notification_Content_Chart_Sync_Removed, removed[0]);
                break;
            default:
                NotificationService.NoticeLight(Notification_Content_Chart_Sync_Summary, added.Count, removed.Count);
                break;
        }
    }
}
