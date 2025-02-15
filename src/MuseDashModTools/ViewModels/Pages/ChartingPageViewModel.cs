using System.Collections.ObjectModel;

namespace MuseDashModTools.ViewModels.Pages;

public sealed partial class ChartingPageViewModel : NavViewModelBase
{
    public override ObservableCollection<NavItem> NavItems { get; } =
    [
        new("Charts", ChartsPanelName),
        new("Charter", CharterPanelName)
    ];

    public ObservableCollection<DropDownButtonItem> DropDownButtons =>
    [
        new("Open",
        [
            new DropDownMenuItem("CustomAlbums Folder", OpenFolderCommand, Setting.CustomAlbumsFolder)
        ])
    ];

    [RelayCommand]
    protected override void Initialize()
    {
        base.Initialize();
        Logger.ZLogInformation($"ChartingPage Initialized");
    }

    [RelayCommand]
    private async Task OpenFolder(string folderPath) => await PlatformService.OpenFolderAsync(folderPath);

    #region Injections

    [UsedImplicitly]
    public ILogger<ModdingPageViewModel> Logger { get; init; } = null!;

    [UsedImplicitly]
    public IPlatformService PlatformService { get; init; } = null!;

    [UsedImplicitly]
    public NavigationService NavigationService { get; init; } = null!;

    [UsedImplicitly]
    public Setting Setting { get; init; } = null!;

    #endregion Injections
}