namespace Euterpe.ViewModels.Pages;

public sealed partial class ModdingPageViewModel : NavViewModelBase
{
    public IReadOnlyList<DropDownButtonItem> DropDownButtons => field ??=
    [
        new(DropDownButton_Open,
        [
            new DropDownMenuItem(Folder_Mods, OpenFolderCommand, GameConfig.ModsFolder),
            new DropDownMenuItem(Folder_UserData, OpenFolderCommand, GameConfig.UserDataFolder),
            new DropDownMenuItem(Folder_UserLibs, OpenFolderCommand, GameConfig.UserLibsFolder)
        ])
    ];

    protected override Task OnInitializeAsync()
    {
        Logger.ZLogInformation($"{nameof(ModdingPageViewModel)} Initialized");
        return base.OnInitializeAsync();
    }

    #region Injections

    [UsedImplicitly]
    public required ILogger<ModdingPageViewModel> Logger { get; init; }

    [UsedImplicitly]
    public required GameConfig GameConfig { get; init; }

    #endregion Injections
}