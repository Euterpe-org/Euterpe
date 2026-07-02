namespace Euterpe.Core;

internal sealed partial class ModManageService
{
    private FolderWatcher? _folderWatcher;

    public void Dispose()
    {
        _folderWatcher?.Dispose();
        _reconcileGate.Dispose();
    }

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
                false,
                ReconcileModsAsync,
                Logger,
                ModFiles.IsModFile);
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to start the mod folder watcher");
        }
    }
}
