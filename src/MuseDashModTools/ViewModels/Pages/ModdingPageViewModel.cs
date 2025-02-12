using System.Collections.ObjectModel;

namespace MuseDashModTools.ViewModels.Pages;

public sealed partial class ModdingPageViewModel : PageViewModelBase
{
    public override ObservableCollection<NavItem> NavItems { get; } =
    [
        new("Mods", ModsPanelName),
        new("Framework", FrameworkPanelName),
        new("Develop", DevelopPanelName)
    ];

    public ObservableCollection<DropDownButtonItem> DropDownButtons =>
    [
        new("Open",
        [
            new("Mods Folder", OpenFolderCommand, Setting.ModsFolder),
            new("UserData Folder", OpenFolderCommand, Setting.UserDataFolder),
            new("UserLib Folder", OpenFolderCommand, Setting.UserLibsFolder)
        ])
    ];

    [RelayCommand]
    protected override void Initialize()
    {
        base.Initialize();
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