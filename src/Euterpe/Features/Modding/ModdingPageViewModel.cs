namespace Euterpe.Features.Modding;

[Route("/modding", DisplayName = Page_Modding, Icon = "Wrench", Order = 1)]
[PerGame]
public sealed partial class ModdingPageViewModel : NavViewModelBase
{
    public IReadOnlyList<DropDownButtonItem> DropDownButtons => field ??=
    [
        new DropDownButtonItem(DropDownButton_Open,
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

    public required ILogger<ModdingPageViewModel> Logger { get; init; }
    public required GameConfig GameConfig { get; init; }

    #endregion Injections
}
