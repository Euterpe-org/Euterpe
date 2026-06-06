using System.Collections.Concurrent;
using Euterpe.Shared.Threading;

namespace Euterpe.Core;

internal sealed partial class ModManageService : IModManageService
{
    private readonly Lazy<Task> _initTask;
    private readonly SingleFlight<string> _singleFlight = new();
    private readonly SourceCache<ModDto, string> _sourceCache = new(x => x.Name);
    private ConcurrentDictionary<string, LibDto> _libsDict = [];

    public ModManageService() => _initTask = new Lazy<Task>(InitializeCoreAsync, LazyThreadSafetyMode.ExecutionAndPublication);

    public IObservable<IChangeSet<ModDto, string>> Connect() => _sourceCache.Connect();

    public Task InitializeModsAsync() => _initTask.Value;

    public ModDto? FindModByName(string name) =>
        _sourceCache.Lookup(name) is { HasValue: true, Value: var mod } ? mod : null;

    public Task InstallModAsync(ModDto mod) => RunExclusiveAsync(mod, () => InstallModCoreAsync(mod));

    public Task UpdateModAsync(ModDto mod) => RunExclusiveAsync(mod, () => UpdateModCoreAsync(mod));

    public Task ReinstallModAsync(ModDto mod) => RunExclusiveAsync(mod, () => ReinstallModCoreAsync(mod));

    public Task UninstallModAsync(ModDto mod) => RunExclusiveAsync(mod, () => UninstallModCoreAsync(mod));

    public Task ToggleModAsync(ModDto mod) => RunExclusiveAsync(mod, () => ToggleModCoreAsync(mod));

    public async Task UpdateAllModsAsync()
    {
        var outdatedMods = _sourceCache.Items.Where(mod => mod.State is ModState.Outdated).ToArray();
        Logger.ZLogInformation($"Updating {outdatedMods.Length} outdated mod(s)");

        foreach (var mod in outdatedMods)
        {
            await UpdateModAsync(mod).ConfigureAwait(false);
        }
    }

    #region Injections

    public required GameConfig GameConfig { get; init; }
    public required IGameDownloadManager GameDownloadManager { get; init; }
    public required IFileSystemService FileSystemService { get; init; }
    public required IModLocalService ModLocalService { get; init; }
    public required ILogger<ModManageService> Logger { get; init; }
    public required INotificationService NotificationService { get; init; }

    #endregion Injections
}