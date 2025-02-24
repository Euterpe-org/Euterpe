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
            new DropDownMenuItem("CustomAlbums Folder", OpenFolderCommand, Config.CustomAlbumsFolder)
        ])
    ];

    protected override void Initialize()
    {
        base.Initialize();
        Logger.ZLogInformation($"{nameof(ChartingPageViewModel)} Initialized");
    }

    #region Injections

    [UsedImplicitly]
    public required ILogger<ModdingPageViewModel> Logger { get; init; }

    [UsedImplicitly]
    public required NavigationService NavigationService { get; init; }

    [UsedImplicitly]
    public required Config Config { get; init; }

    #endregion Injections
}