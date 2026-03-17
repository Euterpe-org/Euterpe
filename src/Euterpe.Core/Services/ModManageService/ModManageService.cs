using System.Collections.Concurrent;
using R3;

namespace Euterpe.Core;

internal sealed partial class ModManageService : IModManageService
{
    private ConcurrentDictionary<string, LibDto> _libsDict = [];
    private SourceCache<ModDto, string> _sourceCache = null!;

    public async Task InitializeModsAsync(SourceCache<ModDto, string> sourceCache)
    {
        _sourceCache = sourceCache;

        Config.ObservePropertyChanged(x => x.MelonLoaderVersion)
            .Subscribe(this, (_, self) => self.RefreshModStates());

        await LoadLibsAsync().ConfigureAwait(false);
        await LoadModsAsync().ConfigureAwait(false);
    }

    public ModDto? FindModByName(string name) =>
        _sourceCache.Lookup(name) is { HasValue: true, Value: var mod } ? mod : null;

    public async Task InstallModAsync(ModDto mod)
    {
        Logger.ZLogInformation($"Installing mod: {mod.Name}");
        await DownloadModCoreAsync(mod).ConfigureAwait(true);
        Logger.ZLogInformation($"Mod {mod.Name} successfully installed");
        NotificationService.SuccessLight(Notification_Content_Mod_Install_Success, mod.Name);
    }

    public async Task UpdateModAsync(ModDto mod)
    {
        Logger.ZLogInformation($"Updating mod: {mod.Name} from version {mod.LocalVersion} to version {mod.Version}");
        File.Delete(Path.Combine(Config.ModsFolder, mod.LocalFileName));
        await DownloadModCoreAsync(mod).ConfigureAwait(true);
        Logger.ZLogInformation($"Mod {mod.Name} successfully updated to version {mod.Version}");
        NotificationService.SuccessLight(Notification_Content_Mod_Update_Success, mod.Name);
    }

    public async Task ReinstallModAsync(ModDto mod)
    {
        Logger.ZLogInformation($"Reinstalling mod: {mod.Name}");
        File.Delete(Path.Combine(Config.ModsFolder, mod.LocalFileName));
        await DownloadModCoreAsync(mod).ConfigureAwait(true);
        Logger.ZLogInformation($"Mod {mod.Name} successfully reinstalled");
        NotificationService.SuccessLight(Notification_Content_Mod_Reinstall_Success, mod.Name);
    }

    public async Task UninstallModAsync(ModDto mod)
    {
        Logger.ZLogInformation($"Uninstalling mod: {mod.Name}");
        File.Delete(Path.Combine(Config.ModsFolder, mod.LocalFileName));
        await DisableModDependentsAsync(mod).ConfigureAwait(true);
        mod.RemoveLocalInfo();
        Logger.ZLogInformation($"Mod {mod.Name} successfully uninstalled");
        NotificationService.SuccessLight(Notification_Content_Mod_Uninstall_Success, mod.Name);
    }

    public Task ToggleModAsync(ModDto mod) => mod.IsDisabled ? EnableModAsync(mod) : DisableModAsync(mod);

    #region Injections

    [UsedImplicitly]
    public required Config Config { get; init; }

    [UsedImplicitly]
    public required IDownloadManager DownloadManager { get; init; }

    [UsedImplicitly]
    public required ILocalService LocalService { get; init; }

    [UsedImplicitly]
    public required ILogger<ModManageService> Logger { get; init; }

    [UsedImplicitly]
    public required INotificationService NotificationService { get; init; }

    [UsedImplicitly]
    public required ITelemetryService TelemetryService { get; init; }

    #endregion Injections
}