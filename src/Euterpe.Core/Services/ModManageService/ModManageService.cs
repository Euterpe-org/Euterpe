using System.Collections.Concurrent;
using AsyncAwaitBestPractices;

namespace Euterpe.Core;

internal sealed partial class ModManageService : IModManageService
{
    private ConcurrentDictionary<string, LibDto> _libsDict = [];
    private SemVersion? _melonLoaderVersion;
    private SourceCache<ModDto, string> _sourceCache = null!;

    public async Task InitializeModsAsync(SourceCache<ModDto, string> sourceCache)
    {
        _sourceCache = sourceCache;

        _melonLoaderVersion = SemVersion.TryParse(Config.MelonLoaderVersion, out var parsedVersion) ? parsedVersion : null;

        await LoadLibsAsync().ConfigureAwait(false);
        await LoadModsAsync().ConfigureAwait(false);
    }

    public async Task InstallModAsync(ModDto mod)
    {
        await DownloadManager.DownloadModAsync(mod).ConfigureAwait(false);
        TelemetryService.TrackDownloadAsync(mod.Name, mod.Author).SafeFireAndForget();
        CheckLibDependencies(mod);
        await EnableModDependenciesAsync(mod).ConfigureAwait(false);
        mod.AddLocalInfo();
    }

    public async Task UninstallModAsync(ModDto mod)
    {
        File.Delete(Path.Combine(Config.ModsFolder, mod.LocalFileName));
        await DisableModDependentsAsync(mod).ConfigureAwait(false);
        mod.RemoveLocalInfo();
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
    public required IMessageBoxService MessageBoxService { get; init; }

    [UsedImplicitly]
    public required ITelemetryService TelemetryService { get; init; }

    #endregion Injections
}