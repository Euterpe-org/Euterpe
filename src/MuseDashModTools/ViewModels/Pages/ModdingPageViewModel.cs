using System.Collections.ObjectModel;

namespace MuseDashModTools.ViewModels.Pages;

public sealed partial class ModdingPageViewModel : NavViewModelBase
{
    public override IReadOnlyList<NavItem> NavItems { get; } =
    [
        new(Panel_Modding_ModManageLiteral, ModManagePanelName),
        new(Panel_Modding_MelonLoaderLiteral, MelonLoaderPanelName),
        new(Panel_Modding_ModDevelopLiteral, ModDevelopPanelName)
    ];

    public ObservableCollection<DropDownButtonItem> DropDownButtons =>
    [
        new(DropDownButton_OpenLiteral,
        [
            new DropDownMenuItem(Folder_ModsLiteral, OpenFolderCommand, Config.ModsFolder),
            new DropDownMenuItem(Folder_UserDataLiteral, OpenFolderCommand, Config.UserDataFolder),
            new DropDownMenuItem(Folder_UserLibsLiteral, OpenFolderCommand, Config.UserLibsFolder)
        ])
    ];

    public override Task InitializeAsync()
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