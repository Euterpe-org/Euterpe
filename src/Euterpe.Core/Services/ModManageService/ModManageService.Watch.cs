namespace Euterpe.Core;

internal sealed partial class ModManageService
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
                [GameConfig.ModsFolder],
                includeSubdirectories: false,
                ReconcileModsAsync,
                Logger,
                isRelevantChange: ModFiles.IsModFile);
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to start the mod folder watcher");
        }
    }

    public void Dispose()
    {
        _folderWatcher?.Dispose();
        _reconcileGate.Dispose();
    }
}
