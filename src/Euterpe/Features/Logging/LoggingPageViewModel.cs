namespace Euterpe.Features.Logging;

[Route("/logging", DisplayName = Page_Logging, Icon = "Terminal", Order = 3)]
[PerGame]
public sealed partial class LoggingPageViewModel : NavViewModelBase
{
    public IReadOnlyList<DropDownButtonItem> DropDownButtons => field ??=
    [
        new DropDownButtonItem(DropDownButton_Open,
        [
            new DropDownMenuItem(Folder_AppLogs, OpenFolderCommand, AppLogsFolder),
            new DropDownMenuItem(Folder_GameLogs, OpenFolderCommand, GameConfig.MelonLoaderFolder)
        ])
    ];

    protected override async Task OnInitializeAsync()
    {
        await base.OnInitializeAsync().ConfigureAwait(false);

        Logger.ZLogInformation($"{nameof(LoggingPageViewModel)} Initialized");
    }

    #region Injections

    public required ILogger<LoggingPageViewModel> Logger { get; init; }
    public required GameConfig GameConfig { get; init; }

    #endregion Injections
}
