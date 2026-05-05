using R3;

namespace Euterpe.Core;

internal sealed partial class ModManageService
{
    private async Task InitializeCoreAsync()
    {
        GameConfig.ObservePropertyChanged(x => x.MelonLoaderVersion)
            .Subscribe(this, (_, self) => self.RefreshModStates());

        await LoadLibsAsync().ConfigureAwait(false);
        await LoadModsAsync().ConfigureAwait(false);
    }

    private async Task<bool> DownloadModCoreAsync(ModDto mod)
    {
        if (!await GameDownloadManager.DownloadModAsync(mod).ConfigureAwait(false))
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

        if (!FileSystemService.TryDeleteFile(Path.Combine(GameConfig.ModsFolder, mod.LocalFileName)))
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

        if (!FileSystemService.TryDeleteFile(Path.Combine(GameConfig.ModsFolder, mod.LocalFileName)))
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
}