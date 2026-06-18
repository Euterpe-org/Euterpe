namespace Euterpe.Features.Charting;

[Route("/charting", DisplayName = Page_Charting, Icon = "Music", Order = 2)]
public sealed partial class ChartingPageViewModel : NavViewModelBase
{
    public IReadOnlyList<DropDownButtonItem> DropDownButtons => field ??=
    [
        new DropDownButtonItem(DropDownButton_Open,
        [
            new DropDownMenuItem(Folder_OnlineCharts, OpenFolderCommand, GameConfig.OnlineChartsFolder),
            new DropDownMenuItem(Folder_OfflineCharts, OpenFolderCommand, GameConfig.OfflineChartsFolder)
        ])
    ];

    protected override Task OnInitializeAsync()
    {
        Logger.ZLogInformation($"{nameof(ChartingPageViewModel)} Initialized");
        return base.OnInitializeAsync();
    }

    #region Injections

    public required ILogger<ModdingPageViewModel> Logger { get; init; }
    public required GameConfig GameConfig { get; init; }

    #endregion Injections
}
