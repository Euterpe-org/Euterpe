namespace Euterpe.Features.Setting;

[Route("/setting/file", DisplayName = Panel_Setting_FileManagement, Order = 3)]
[PerGame]
public sealed partial class FileManagementPanelViewModel : ViewModelBase
{
    [RelayCommand]
    private async Task ChangeGameFolderAsync() =>
        GameConfig.Folder = await GameLocalService.GetGameFolderAsync().ConfigureAwait(false);

    [RelayCommand]
    private async Task ChangeCacheFolderAsync() =>
        Config.CacheFolder = await AppLocalService.GetCacheFolderAsync().ConfigureAwait(false);

    #region Injections

    public required IAppLocalService AppLocalService { get; init; }
    public required Config Config { get; init; }
    public required GameConfig GameConfig { get; init; }
    public required IGameLocalService GameLocalService { get; init; }

    #endregion Injections
}