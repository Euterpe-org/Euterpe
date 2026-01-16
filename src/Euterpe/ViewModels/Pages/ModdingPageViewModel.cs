namespace Euterpe.ViewModels.Pages;

public sealed partial class ModdingPageViewModel : NavViewModelBase
{
    public override IReadOnlyList<NavItem> NavItems { get; } =
    [
        new(Panel_Modding_ModManage, ModManagePanelName),
        new(Panel_Modding_MelonLoader, MelonLoaderPanelName),
        new(Panel_Modding_ModDevelop, ModDevelopPanelName)
    ];

    public IReadOnlyList<DropDownButtonItem> DropDownButtons =>
    [
        new(DropDownButton_Open,
        [
            new DropDownMenuItem(Folder_Mods, OpenFolderCommand, Config.ModsFolder),
            new DropDownMenuItem(Folder_UserData, OpenFolderCommand, Config.UserDataFolder),
            new DropDownMenuItem(Folder_UserLibs, OpenFolderCommand, Config.UserLibsFolder)
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
    public required Config Config { get; init; }

    #endregion Injections
}