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

    public Task InstallModAsync(ModDto mod) =>
        _singleFlight.RunAsync(mod.Name, () => InstallModCoreAsync(mod));

    public Task UpdateModAsync(ModDto mod) =>
        _singleFlight.RunAsync(mod.Name, () => UpdateModCoreAsync(mod));

    public Task ReinstallModAsync(ModDto mod) =>
        _singleFlight.RunAsync(mod.Name, () => ReinstallModCoreAsync(mod));

    public Task UninstallModAsync(ModDto mod) =>
        _singleFlight.RunAsync(mod.Name, () => UninstallModCoreAsync(mod));

    public Task ToggleModAsync(ModDto mod) =>
        _singleFlight.RunAsync(mod.Name, () => ToggleModCoreAsync(mod));

    #region Injections

    [UsedImplicitly]
    public required GameConfig GameConfig { get; init; }

    [UsedImplicitly]
    public required IGameDownloadManager GameDownloadManager { get; init; }

    [UsedImplicitly]
    public required IFileSystemService FileSystemService { get; init; }

    [UsedImplicitly]
    public required IGameLocalService GameLocalService { get; init; }

    [UsedImplicitly]
    public required ILogger<ModManageService> Logger { get; init; }

    [UsedImplicitly]
    public required INotificationService NotificationService { get; init; }

    #endregion Injections
}