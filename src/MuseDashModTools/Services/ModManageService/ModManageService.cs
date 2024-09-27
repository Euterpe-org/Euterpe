using DynamicData;

namespace MuseDashModTools.Services;

public sealed partial class ModManageService : IModManageService
{
    private string? _gameVersion;
    private SourceCache<ModDto, string>? _sourceCache;

    [MemberNotNull(nameof(_gameVersion))]
    [MemberNotNull(nameof(_sourceCache))]
    public async Task InitializeModsAsync(SourceCache<ModDto, string> sourceCache)
    {
        _sourceCache = sourceCache;
        _gameVersion = await LocalService.ReadGameVersionAsync().ConfigureAwait(true);

        IEnumerable<ModDto> localMods = LocalService.GetModFilePaths()
            .Select(LocalService.LoadModFromPath)
            .Where(mod => mod is not null)!;

        _sourceCache.AddOrUpdate(localMods);

        await foreach (var webMod in DownloadManager.GetModListAsync())
        {
            if (webMod is null)
            {
                continue;
            }

            if (sourceCache.Lookup(webMod.Name) is { HasValue: true, Value: var localMod })
            {
                CheckModState(localMod, webMod);
                localMod.UpdateFromMod(webMod);
                sourceCache.AddOrUpdate(localMod);
            }
            else
            {
                sourceCache.AddOrUpdate(new ModDto(webMod));
            }
        }
    }

    #region Injections

    [UsedImplicitly]
    public IDownloadManager DownloadManager { get; init; } = null!;

    [UsedImplicitly]
    public ILocalService LocalService { get; init; } = null!;

    [UsedImplicitly]
    public ILogger Logger { get; init; } = null!;

    #endregion Injections
}