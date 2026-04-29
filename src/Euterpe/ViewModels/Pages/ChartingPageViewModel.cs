namespace Euterpe.ViewModels.Pages;

public sealed partial class ChartingPageViewModel : NavViewModelBase
{
    public IReadOnlyList<DropDownButtonItem> DropDownButtons =>
    [
        new(DropDownButton_Open,
        [
            new DropDownMenuItem(Folder_OnlineCharts, OpenFolderCommand, Config.OnlineChartsFolder),
            new DropDownMenuItem(Folder_OfflineCharts, OpenFolderCommand, Config.OfflineChartsFolder)
        ])
    ];

    protected override Task OnInitializeAsync()
    {
        Logger.ZLogInformation($"{nameof(ChartingPageViewModel)} Initialized");
        return base.OnInitializeAsync();
    }

    #region Injections

    [UsedImplicitly]
    public required ILogger<ModdingPageViewModel> Logger { get; init; }

    [UsedImplicitly]
    public required Config Config { get; init; }

    #endregion Injections
}