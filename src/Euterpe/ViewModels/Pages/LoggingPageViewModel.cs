namespace Euterpe.ViewModels.Pages;

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

    [UsedImplicitly]
    public required ILogger<LoggingPageViewModel> Logger { get; init; }

    [UsedImplicitly]
    public required GameConfig GameConfig { get; init; }

    #endregion Injections
}