namespace MuseDashModTools.ViewModels.Pages;

public sealed partial class LoggingPageViewModel : NavViewModelBase
{
    public override IReadOnlyList<NavItem> NavItems { get; } =
    [
        new(Page_Logging, LoggingPageName)
    ];

    #region Injections

    [UsedImplicitly]
    public required ILogger<LoggingPageViewModel> Logger { get; init; }

    #endregion Injections
}