using System.Collections.ObjectModel;

namespace MuseDashModTools.ViewModels.Pages;

public sealed partial class ModdingPageViewModel : NavViewModelBase
{
    public override ObservableCollection<NavItem> NavItems { get; } =
    [
        new("Mods", ModsPanelName),
        new("Melon Loader", MelonLoaderPanelName),
        new("Develop", DevelopPanelName)
    ];

    public ObservableCollection<DropDownButtonItem> DropDownButtons =>
    [
        new("Open",
        [
            new DropDownMenuItem("Mods Folder", OpenFolderCommand, Config.ModsFolder),
            new DropDownMenuItem("UserData Folder", OpenFolderCommand, Config.UserDataFolder),
            new DropDownMenuItem("UserLib Folder", OpenFolderCommand, Config.UserLibsFolder)
        ])
    ];

    [RelayCommand]
    protected override void Initialize()
    {
        base.Initialize();
        Logger.ZLogInformation($"ModdingPage Initialized");
    }

    [RelayCommand]
    private Task OpenFolderAsync(string folderPath) => PlatformService.OpenFolderAsync(folderPath);

    #region Injections

    [UsedImplicitly]
    public ILogger<ModdingPageViewModel> Logger { get; init; } = null!;

    [UsedImplicitly]
    public IPlatformService PlatformService { get; init; } = null!;

    [UsedImplicitly]
    public NavigationService NavigationService { get; init; } = null!;

    [UsedImplicitly]
    public Config Config { get; init; } = null!;

    #endregion Injections
}