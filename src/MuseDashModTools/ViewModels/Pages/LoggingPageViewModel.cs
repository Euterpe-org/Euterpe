namespace MuseDashModTools.ViewModels.Pages;

public sealed partial class LoggingPageViewModel : NavViewModelBase
{
    public override IReadOnlyList<NavItem> NavItems { get; } =
    [
        new(Panel_Logging_LogViewer, LogViewerPanelName)
    ];

    public IReadOnlyList<DropDownButtonItem> DropDownButtons =>
    [
        new(DropDownButton_Open,
        [
            new DropDownMenuItem(Folder_AppLogs, OpenFolderCommand, AppLogsFolder)
        ])
    ];

    #region Injections

    [UsedImplicitly]
    public required ILogger<LoggingPageViewModel> Logger { get; init; }

    #endregion Injections

    public override Task InitializeAsync()
    {
        base.InitializeAsync();

        Logger.ZLogInformation($"{nameof(LoggingPageViewModel)} Initialized");
        return Task.CompletedTask;
    }
}