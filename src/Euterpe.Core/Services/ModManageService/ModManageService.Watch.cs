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
                MapModFile,
                _ => ReconcileModsAsync(),
                ReconcileModsAsync,
                FolderWatcher.DefaultDebounce,
                Logger,
                nameof(ModManageService));
            _folderWatcher.Start();
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to start the mod folder watcher");
        }
    }

    private static string? MapModFile(string fullPath) =>
        ModFiles.IsModFile(fullPath) ? fullPath : null;

    public void Dispose()
    {
        _folderWatcher?.Dispose();
        _reconcileGate.Dispose();
    }
}
