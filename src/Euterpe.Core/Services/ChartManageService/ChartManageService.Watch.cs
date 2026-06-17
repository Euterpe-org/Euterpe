namespace Euterpe.Core;

internal sealed partial class ChartManageService
{
    private FolderWatcher? _folderWatcher;

    private void StartWatching()
    {
        if (!GameConfig.SetupCompleted)
        {
            return;
        }

        try
        {
            _folderWatcher = new FolderWatcher(
                [GameConfig.OnlineChartsFolder, GameConfig.OfflineChartsFolder],
                includeSubdirectories: true,
                MapChartFolder,
                ReconcileChartsAsync,
                ReconcileChartsAsync,
                FolderWatcher.DefaultDebounce,
                Logger,
                nameof(ChartManageService));
            _folderWatcher.Start();
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to start the chart folder watcher");
        }
    }

    private string? MapChartFolder(string fullPath) =>
        MapTopLevelFolder(GameConfig.OnlineChartsFolder, fullPath) ?? MapTopLevelFolder(GameConfig.OfflineChartsFolder, fullPath);

    private static string? MapTopLevelFolder(string root, string fullPath)
    {
        if (!fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var relative = fullPath[root.Length..].TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        if (relative.Length == 0)
        {
            return null;
        }

        var separator = relative.IndexOfAny([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar]);
        return Path.Combine(root, separator < 0 ? relative : relative[..separator]);
    }

    public void Dispose() => _folderWatcher?.Dispose();
}
