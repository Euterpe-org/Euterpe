namespace Euterpe.ViewModels.Panels.Setting;

public sealed partial class FileManagementPanelViewModel : ViewModelBase
{
    [RelayCommand]
    private async Task ChangeMuseDashFolderAsync() =>
        Config.MuseDashFolder = await LocalService.GetMuseDashFolderAsync().ConfigureAwait(false);

    [RelayCommand]
    private async Task ChangeCacheFolderAsync() =>
        Config.CacheFolder = await LocalService.GetCacheFolderAsync().ConfigureAwait(false);

    #region Injections

    [UsedImplicitly]
    public required Config Config { get; init; }

    [UsedImplicitly]
    public required ILocalService LocalService { get; init; }

    [UsedImplicitly]
    public required ILogger<FileManagementPanelViewModel> Logger { get; init; }

    #endregion Injections
}