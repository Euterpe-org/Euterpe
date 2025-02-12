using System.Collections.ObjectModel;

namespace MuseDashModTools.ViewModels.Pages;

public sealed partial class ModdingPageViewModel : PageViewModelBase
{
    public override ObservableCollection<NavItem> NavItems { get; } =
    [
        new("Mods", "", ModsPanelName),
        new("Framework", "", FrameworkPanelName),
        new("Develop", "", DevelopPanelName)
    ];

    public ObservableCollection<DropDownButtonItem> DropDownButtons { get; } = [];

    [RelayCommand]
    protected override void Initialize()
    {
        base.Initialize();

        DropDownButtons.Add(new("Browse",
        [
            new DropDownItem("Mods Folder", new RelayCommand(() => LocalService.BrowseFolderAsync(Setting.ModsFolder))),
            new DropDownItem("UserData Folder", new RelayCommand(() => LocalService.BrowseFolderAsync(Setting.UserDataFolder))),
            new DropDownItem("UserLib Folder", new RelayCommand(() => LocalService.BrowseFolderAsync(Setting.UserLibsFolder)))
        ]));

        Logger.ZLogInformation($"ModdingPage Initialized");
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