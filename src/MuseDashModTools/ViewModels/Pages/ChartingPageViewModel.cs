using System.Collections.ObjectModel;

namespace MuseDashModTools.ViewModels.Pages;

public sealed partial class ChartingPageViewModel : PageViewModelBase
{
    public override ObservableCollection<NavItem> NavItems { get; } =
    [
        new("Charts", "", ChartsPanelName),
        new("Charter", "", CharterPanelName)
    ];

    public ObservableCollection<DropDownButtonItem> DropDownButtons { get; } = [];

    [RelayCommand]
    protected override void Initialize()
    {
        base.Initialize();

        DropDownButtons.Add(new("Browse",
        [
            new DropDownItem("CustomAlbums Folder", new RelayCommand(() => LocalService.BrowseFolderAsync(Setting.CustomAlbumsFolder)))
        ]));

        Logger.ZLogInformation($"ChartingPage Initialized");
    }

    #region Injections

    [UsedImplicitly]
    public NavigationService NavigationService { get; init; } = null!;

    [UsedImplicitly]
    public ILocalService LocalService { get; init; } = null!;

    [UsedImplicitly]
    public Setting Setting { get; init; } = null!;

    [UsedImplicitly]
    public ILogger<ModdingPageViewModel> Logger { get; init; } = null!;

    #endregion Injections
}