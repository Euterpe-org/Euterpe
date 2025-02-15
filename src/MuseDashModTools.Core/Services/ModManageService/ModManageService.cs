using DynamicData;

namespace MuseDashModTools.Core;

internal sealed partial class ModManageService : IModManageService
{
    private string _gameVersion = null!;
    private SourceCache<ModDto, string> _sourceCache = null!;

    public async Task InitializeModsAsync(SourceCache<ModDto, string> sourceCache)
    {
        _sourceCache = sourceCache;
        _gameVersion = await LocalService.ReadGameVersionAsync().ConfigureAwait(false);

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

    #region Injections

    [UsedImplicitly]
    public Setting Setting { get; init; } = null!;

    [UsedImplicitly]
    public IDownloadManager DownloadManager { get; init; } = null!;

    [UsedImplicitly]
    public ILocalService LocalService { get; init; } = null!;

    [UsedImplicitly]
    public ILogger<ModManageService> Logger { get; init; } = null!;

    #endregion Injections
}