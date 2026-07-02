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
                ReconcileChartsAsync,
                Logger);
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to start the chart folder watcher");
        }
    }

    public void Dispose() => _folderWatcher?.Dispose();
}
