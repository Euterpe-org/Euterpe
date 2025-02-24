using System.Collections.ObjectModel;

namespace MuseDashModTools.ViewModels.Pages;

public sealed partial class ModdingPageViewModel : NavViewModelBase
{
    public override IReadOnlyList<NavItem> NavItems { get; } =
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

    protected override Task OnActivatedAsync(CompositeDisposable disposables)
    {
        base.OnActivatedAsync(disposables);

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