namespace Euterpe.ViewModels.Pages;

public sealed partial class ChartingPageViewModel : NavViewModelBase
{
    public IReadOnlyList<DropDownButtonItem> DropDownButtons =>
    [
        new(DropDownButton_Open,
        [
            new DropDownMenuItem(Folder_CustomAlbums, OpenFolderCommand, Config.CustomAlbumsFolder)
        ])
    ];

    public override Task InitializeAsync()
    {
        base.InitializeAsync();

        Logger.ZLogInformation($"{nameof(ChartingPageViewModel)} Initialized");
        return Task.CompletedTask;
    }

    #region Injections

    [UsedImplicitly]
    public required ILogger<ModdingPageViewModel> Logger { get; init; }

    [UsedImplicitly]
    public required Config Config { get; init; }

    #endregion Injections
}