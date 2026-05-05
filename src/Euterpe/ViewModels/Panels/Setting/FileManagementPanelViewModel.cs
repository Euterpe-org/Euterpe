namespace Euterpe.ViewModels.Panels.Setting;

public sealed partial class FileManagementPanelViewModel : ViewModelBase
{
    [RelayCommand]
    private async Task ChangeGameFolderAsync() =>
        GameConfig.Folder = await GameLocalService.GetGameFolderAsync().ConfigureAwait(false);

    [RelayCommand]
    private async Task ChangeCacheFolderAsync() =>
        Config.CacheFolder = await AppLocalService.GetCacheFolderAsync().ConfigureAwait(false);

    #region Injections

    [UsedImplicitly]
    public required IAppLocalService AppLocalService { get; init; }

    [UsedImplicitly]
    public required Config Config { get; init; }

    [UsedImplicitly]
    public required GameConfig GameConfig { get; init; }

    [UsedImplicitly]
    public required IGameLocalService GameLocalService { get; init; }

    [UsedImplicitly]
    public required ILogger<FileManagementPanelViewModel> Logger { get; init; }

    #endregion Injections
}