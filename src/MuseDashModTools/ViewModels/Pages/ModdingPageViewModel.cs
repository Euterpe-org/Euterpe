using System.Collections.ObjectModel;

namespace MuseDashModTools.ViewModels.Pages;

public sealed partial class ModdingPageViewModel : NavViewModelBase
{
    public override IReadOnlyList<NavItem> NavItems { get; } =
    [
        new(XAML_Panel_Modding_ModManage, ModManagePanelName),
        new(XAML_Panel_Modding_MelonLoader, MelonLoaderPanelName),
        new(XAML_Panel_Modding_ModDevelop, ModDevelopPanelName)
    ];

    public ObservableCollection<DropDownButtonItem> DropDownButtons =>
    [
        new("Open",
        [
            new DropDownMenuItem(XAML_Folder_Mods, OpenFolderCommand, Config.ModsFolder),
            new DropDownMenuItem(XAML_Folder_UserData, OpenFolderCommand, Config.UserDataFolder),
            new DropDownMenuItem(XAML_Folder_UserLibs, OpenFolderCommand, Config.UserLibsFolder)
        ])
    ];

    [RelayCommand]
    protected override Task InitializeAsync()
    {
        base.InitializeAsync();

        Logger.ZLogInformation($"{nameof(ModdingPageViewModel)} Initialized");
        return Task.CompletedTask;
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