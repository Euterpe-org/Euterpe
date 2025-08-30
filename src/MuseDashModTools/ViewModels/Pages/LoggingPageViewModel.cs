namespace MuseDashModTools.ViewModels.Pages;

public sealed partial class LoggingPageViewModel : NavViewModelBase
{
    public override IReadOnlyList<NavItem> NavItems { get; } =
    [
        new(Panel_Logging_LogViewer, LogViewerPanelName)
    ];

    #region Injections

    [UsedImplicitly]
    public required ILogger<LoggingPageViewModel> Logger { get; init; }

    #endregion Injections
}