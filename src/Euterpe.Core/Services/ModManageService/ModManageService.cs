using System.Collections.Concurrent;

namespace Euterpe.Core;

internal sealed partial class ModManageService : IModManageService
{
    private readonly Lazy<Task> _initTask;
    private readonly SourceCache<ModDto, string> _sourceCache = new(x => x.Name);
    private ConcurrentDictionary<string, LibDto> _libsDict = [];

    public ModManageService() => _initTask = new Lazy<Task>(InitializeCoreAsync, LazyThreadSafetyMode.ExecutionAndPublication);

    public IObservable<IChangeSet<ModDto, string>> Connect() => _sourceCache.Connect();

    public Task InitializeModsAsync() => _initTask.Value;

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

    public Task ToggleModAsync(ModDto mod)
    {
        Logger.ZLogInformation($"Toggling mod: {mod.Name}");
        return mod.IsDisabled ? EnableModAsync(mod) : DisableModAsync(mod);
    }

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

    #endregion Injections
}