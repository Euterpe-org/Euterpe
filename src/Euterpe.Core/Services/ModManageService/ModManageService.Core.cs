using R3;

namespace Euterpe.Core;

internal sealed partial class ModManageService
{
    private async Task InitializeCoreAsync()
    {
        GameConfig.ObservePropertyChanged(x => x.MelonLoaderVersion, false)
            .SubscribeAwait(this, static (_, self, _) => self.ReconcileAfterMelonLoaderChangeAsync());

        await LoadLibsAsync().ConfigureAwait(false);

        await _reconcileGate.AcquireAsync().ConfigureAwait(false);
        try
        {
            CacheLocalMods(await LoadLocalModsAsync().ConfigureAwait(false));
            await LoadWebModsAsync().ConfigureAwait(false);
            RefreshModStatesCore();
        }
        finally
        {
            _reconcileGate.Release();
        }

        StartWatching();
    }

    private async ValueTask ReconcileAfterMelonLoaderChangeAsync()
    {
        try
        {
            await ReconcileModsAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to reconcile mods after MelonLoader version change");
        }
    }

    private async Task DownloadModCoreAsync(ModDto mod)
    {
        await GameDownloadManager.DownloadModAsync(mod).ConfigureAwait(false);
        CheckLibDependencies(mod);
        await EnableModDependenciesAsync(mod).ConfigureAwait(false);
        mod.AddLocalInfo();
    }

    private async Task ReplaceModCoreAsync(ModDto mod)
    {
        var previousFileName = mod.LocalFileName;

        await DownloadModCoreAsync(mod).ConfigureAwait(false);

        if (!string.Equals(previousFileName, mod.FileName, StringComparison.OrdinalIgnoreCase))
        {
            FileSystemService.TryDeleteFile(Path.Combine(GameConfig.ModsFolder, previousFileName), DeleteOption.IgnoreIfNotFound);
        }
    }

    private async Task InstallModCoreAsync(ModDto mod)
    {
        Logger.ZLogInformation($"Installing mod: {mod.Name}");

        try
        {
            await DownloadModCoreAsync(mod).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to install mod {mod.Name}");
            NotificationService.ErrorLight(Notification_Content_Mod_Install_Failed, mod.Name);
            return;
        }

        Logger.ZLogInformation($"Mod {mod.Name} successfully installed");
        NotificationService.SuccessLight(Notification_Content_Mod_Install_Success, mod.Name);
    }

    private async Task<bool> UpdateModCoreAsync(ModDto mod)
    {
        Logger.ZLogInformation($"Updating mod: {mod.Name} from version {mod.LocalVersion} to version {mod.Version}");

        try
        {
            await ReplaceModCoreAsync(mod).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to update mod {mod.Name}");
            return false;
        }

        Logger.ZLogInformation($"Mod {mod.Name} successfully updated to version {mod.Version}");
        return true;
    }

    private async Task ReinstallModCoreAsync(ModDto mod)
    {
        Logger.ZLogInformation($"Reinstalling mod: {mod.Name}");

        try
        {
            await ReplaceModCoreAsync(mod).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to reinstall mod {mod.Name}");
            NotificationService.ErrorLight(Notification_Content_Mod_Reinstall_Failed, mod.Name);
            return;
        }

        Logger.ZLogInformation($"Mod {mod.Name} successfully reinstalled");
        NotificationService.SuccessLight(Notification_Content_Mod_Reinstall_Success, mod.Name);
    }

    private async Task UninstallModCoreAsync(ModDto mod)
    {
        Logger.ZLogInformation($"Uninstalling mod: {mod.Name}");

        if (!FileSystemService.TryDeleteFile(Path.Combine(GameConfig.ModsFolder, mod.LocalFileName)))
        {
            Logger.ZLogError($"Failed to uninstall mod {mod.Name}: could not delete file {mod.LocalFileName}");
            NotificationService.ErrorLight(Notification_Content_Mod_Uninstall_Failed, mod.Name);
            return;
        }

        await DisableModDependentsAsync(mod).ConfigureAwait(false);
        mod.RemoveLocalInfo();

        Logger.ZLogInformation($"Mod {mod.Name} successfully uninstalled");
        NotificationService.SuccessLight(Notification_Content_Mod_Uninstall_Success, mod.Name);
    }

    private async Task ToggleModCoreAsync(ModDto mod)
    {
        Logger.ZLogInformation($"Toggling mod: {mod.Name}");

        var success = mod.IsDisabled
            ? await EnableModAsync(mod).ConfigureAwait(false)
            : await DisableModAsync(mod).ConfigureAwait(false);

        if (!success)
        {
            NotificationService.ErrorLight(Notification_Content_Mod_Toggle_Failed, mod.Name);
        }
    }

    private async Task RunAndRefreshAsync(ModDto mod, Func<Task> action)
    {
        await RunExclusiveAsync(mod, async () =>
        {
            await action().ConfigureAwait(false);
            return true;
        }).ConfigureAwait(false);

        await RefreshModStatesAsync().ConfigureAwait(false);
    }

    private async Task<T> RunExclusiveAsync<T>(ModDto mod, Func<Task<T>> action)
    {
        T result = default!;
        await _singleFlight.RunAsync(mod.Name, async () =>
        {
            mod.IsProcessing = true;
            try
            {
                result = await action().ConfigureAwait(false);
            }
            finally
            {
                mod.IsProcessing = false;
            }
        }).ConfigureAwait(false);
        return result;
    }
}
