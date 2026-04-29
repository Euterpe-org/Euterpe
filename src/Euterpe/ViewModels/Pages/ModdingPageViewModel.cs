namespace Euterpe.ViewModels.Pages;

public sealed partial class ModdingPageViewModel : NavViewModelBase
{
    public IReadOnlyList<DropDownButtonItem> DropDownButtons =>
    [
        new(DropDownButton_Open,
        [
            new DropDownMenuItem(Folder_Mods, OpenFolderCommand, Config.ModsFolder),
            new DropDownMenuItem(Folder_UserData, OpenFolderCommand, Config.UserDataFolder),
            new DropDownMenuItem(Folder_UserLibs, OpenFolderCommand, Config.UserLibsFolder)
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
    public required Config Config { get; init; }

    #endregion Injections
}