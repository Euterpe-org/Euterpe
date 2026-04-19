using R3;

namespace Euterpe.Core;

internal sealed partial class ModManageService
{
    private async Task InitializeCoreAsync()
    {
        Config.ObservePropertyChanged(x => x.MelonLoaderVersion)
            .Subscribe(this, (_, self) => self.RefreshModStates());

        await LoadLibsAsync().ConfigureAwait(false);
        await LoadModsAsync().ConfigureAwait(false);
    }

    private async Task<bool> DownloadModCoreAsync(ModDto mod)
    {
        if (!await DownloadManager.DownloadModAsync(mod).ConfigureAwait(false))
        {
            return false;
        }

        CheckLibDependencies(mod);
        await EnableModDependenciesAsync(mod).ConfigureAwait(false);
        mod.AddLocalInfo();

        return true;
    }

    private async Task InstallModCoreAsync(ModDto mod)
    {
        Logger.ZLogInformation($"Installing mod: {mod.Name}");

        if (!await DownloadModCoreAsync(mod).ConfigureAwait(false))
        {
            Logger.ZLogError($"Failed to install mod {mod.Name}: download failed");
            NotificationService.ErrorLight(Notification_Content_Mod_Install_Failed, mod.Name);
            return;
        }

        Logger.ZLogInformation($"Mod {mod.Name} successfully installed");
        NotificationService.SuccessLight(Notification_Content_Mod_Install_Success, mod.Name);
    }

    private async Task UpdateModCoreAsync(ModDto mod)
    {
        Logger.ZLogInformation($"Updating mod: {mod.Name} from version {mod.LocalVersion} to version {mod.Version}");

        if (!FileSystemService.TryDeleteFile(Path.Combine(Config.ModsFolder, mod.LocalFileName)))
        {
            Logger.ZLogError($"Failed to update mod {mod.Name}: could not delete existing file {mod.LocalFileName}");
            NotificationService.ErrorLight(Notification_Content_Mod_Update_Failed, mod.Name);
            return;
        }

        if (!await DownloadModCoreAsync(mod).ConfigureAwait(false))
        {
            Logger.ZLogError($"Failed to update mod {mod.Name}: download failed");
            NotificationService.ErrorLight(Notification_Content_Mod_Update_Failed, mod.Name);
            return;
        }

        Logger.ZLogInformation($"Mod {mod.Name} successfully updated to version {mod.Version}");
        NotificationService.SuccessLight(Notification_Content_Mod_Update_Success, mod.Name);
    }

    private async Task ReinstallModCoreAsync(ModDto mod)
    {
        Logger.ZLogInformation($"Reinstalling mod: {mod.Name}");

        if (!FileSystemService.TryDeleteFile(Path.Combine(Config.ModsFolder, mod.LocalFileName)))
        {
            Logger.ZLogError($"Failed to reinstall mod {mod.Name}: could not delete existing file {mod.LocalFileName}");
            NotificationService.ErrorLight(Notification_Content_Mod_Reinstall_Failed, mod.Name);
            return;
        }

        if (!await DownloadModCoreAsync(mod).ConfigureAwait(false))
        {
            Logger.ZLogError($"Failed to reinstall mod {mod.Name}: download failed");
            NotificationService.ErrorLight(Notification_Content_Mod_Reinstall_Failed, mod.Name);
            return;
        }

        Logger.ZLogInformation($"Mod {mod.Name} successfully reinstalled");
        NotificationService.SuccessLight(Notification_Content_Mod_Reinstall_Success, mod.Name);
    }

    private async Task UninstallModCoreAsync(ModDto mod)
    {
        Logger.ZLogInformation($"Uninstalling mod: {mod.Name}");

        if (!FileSystemService.TryDeleteFile(Path.Combine(Config.ModsFolder, mod.LocalFileName)))
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
        if (mod.IsDisabled)
        {
            await EnableModAsync(mod).ConfigureAwait(false);
        }
        else
        {
            await DisableModAsync(mod).ConfigureAwait(false);
        }
    }

    private async Task EnableModAsync(ModDto mod)
    {
        File.Move(Path.Combine(Config.ModsFolder, mod.LocalFileName),
            Path.Combine(Config.ModsFolder, mod.ReversedFileName));

        CheckLibDependencies(mod);
        await EnableModDependenciesAsync(mod).ConfigureAwait(false);

        Logger.ZLogInformation($"Change mod {mod.Name} state to enabled");
        mod.IsDisabled = false;
    }

    private async Task EnableModDependenciesAsync(ModDto mod)
    {
        var modDependencies = FindModDependencies(mod);
        foreach (var dependency in modDependencies)
        {
            if (dependency is { IsDisabled: true, IsLocal: true })
            {
                await EnableModAsync(dependency).ConfigureAwait(false);
            }
            else if (!dependency.IsLocal)
            {
                await InstallModAsync(dependency).ConfigureAwait(false);
            }
        }
    }

    private async Task DisableModAsync(ModDto mod)
    {
        File.Move(Path.Combine(Config.ModsFolder, mod.LocalFileName),
            Path.Combine(Config.ModsFolder, mod.ReversedFileName));

        await DisableModDependentsAsync(mod).ConfigureAwait(false);

        Logger.ZLogInformation($"Change mod {mod.Name} state to disabled");
        mod.IsDisabled = true;
    }

    private async Task DisableModDependentsAsync(ModDto mod)
    {
        var modDependents = FindModDependents(mod);
        foreach (var dependent in modDependents)
        {
            if (dependent is { IsDisabled: false, IsLocal: true })
            {
                await DisableModAsync(dependent).ConfigureAwait(false);
            }
        }
    }
}