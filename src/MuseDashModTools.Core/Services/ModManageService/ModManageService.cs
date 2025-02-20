using DynamicData;

namespace MuseDashModTools.Core;

internal sealed partial class ModManageService : IModManageService
{
    private string _gameVersion = null!;
    private Dictionary<string, LibDto> _libsDict = [];
    private SourceCache<ModDto, string> _sourceCache = null!;

    public async Task InitializeModsAsync(SourceCache<ModDto, string> sourceCache)
    {
        _sourceCache = sourceCache;
        _gameVersion = await LocalService.ReadGameVersionAsync().ConfigureAwait(false);

        await LoadModsAsync().ConfigureAwait(false);
        await LoadLibsAsync().ConfigureAwait(false);
    }

    public async Task InstallModAsync(ModDto mod)
    {
        await DownloadManager.DownloadModAsync(mod).ConfigureAwait(false);
    }

    public Task UninstallModAsync(ModDto mod) => throw new NotImplementedException();

    public Task ToggleModAsync(ModDto mod) => mod.IsDisabled ? EnableModAsync(mod) : DisableModAsync(mod);

    #region Injections

    [UsedImplicitly]
    public Config Config { get; init; } = null!;

    [UsedImplicitly]
    public WindowNotificationManager WindowNotificationManager { get; init; } = null!;

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