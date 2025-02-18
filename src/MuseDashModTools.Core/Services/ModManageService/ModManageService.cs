using DynamicData;

namespace MuseDashModTools.Core;

internal sealed partial class ModManageService : IModManageService
{
    private string _gameVersion = null!;
    private HashSet<string> _libNames = [];
    private SourceCache<ModDto, string> _sourceCache = null!;

    public async Task InitializeModsAsync(SourceCache<ModDto, string> sourceCache)
    {
        _sourceCache = sourceCache;
        _gameVersion = await LocalService.ReadGameVersionAsync().ConfigureAwait(false);
        _libNames = LocalService.GetLibFileNames().ToHashSet();

        ModDto[] localMods = LocalService.GetModFilePaths()
            .Select(LocalService.LoadModFromPath)
            .Where(mod => mod is not null)
            .ToArray()!;

        _sourceCache.AddOrUpdate(localMods);
        Logger.ZLogInformation($"Local mods added to source cache");

        CheckDuplicatedMods(localMods);

        await foreach (var webMod in DownloadManager.GetModListAsync())
        {
            if (webMod is null)
            {
                continue;
            }

            if (_sourceCache.Lookup(webMod.Name) is { HasValue: true, Value: var localMod })
            {
                CheckModState(localMod, webMod);
                localMod.UpdateFromMod(webMod);
                CheckConfigFile(localMod);
                _sourceCache.AddOrUpdate(localMod);
            }
            else
            {
                _sourceCache.AddOrUpdate(webMod.ToDto());
            }
        }

        Logger.ZLogInformation($"Updated mod info from web completed");
    }

    public async Task InstallModAsync(ModDto mod)
    {
        await DownloadManager.DownloadModAsync(mod);
    }

    public Task UninstallModAsync(ModDto mod) => throw new NotImplementedException();

    public Task UpdateModAsync(ModDto mod) => throw new NotImplementedException();

    public Task ToggleModAsync(ModDto mod) => mod.IsDisabled ? EnableModAsync(mod) : DisableModAsync(mod);

    #region Injections

    [UsedImplicitly]
    public Config Config { get; init; } = null!;

    [UsedImplicitly]
    public IDownloadManager DownloadManager { get; init; } = null!;

    [UsedImplicitly]
    public ILocalService LocalService { get; init; } = null!;

    [UsedImplicitly]
    public ILogger<ModManageService> Logger { get; init; } = null!;

    [UsedImplicitly]
    public IMessageBoxService MessageBoxService { get; init; } = null!;

    #endregion Injections
}