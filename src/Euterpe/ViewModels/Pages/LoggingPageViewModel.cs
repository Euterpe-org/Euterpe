namespace Euterpe.ViewModels.Pages;

public sealed partial class LoggingPageViewModel : NavViewModelBase
{
    public IReadOnlyList<DropDownButtonItem> DropDownButtons => field ??=
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

    protected override Task OnInitializeAsync()
    {
        Logger.ZLogInformation($"{nameof(LoggingPageViewModel)} Initialized");
        return base.OnInitializeAsync();
    }
}