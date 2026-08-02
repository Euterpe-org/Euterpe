namespace Euterpe.Core;

internal sealed partial class ChartManageService
{
    private FolderWatcher? _folderWatcher;

    public void Dispose() => _folderWatcher?.Dispose();

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
                true,
                ReconcileChartsAsync,
                Logger);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to start the chart folder watcher");
        }
    }
}
