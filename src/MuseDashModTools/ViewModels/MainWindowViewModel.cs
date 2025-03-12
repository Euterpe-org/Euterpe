namespace MuseDashModTools.ViewModels;

public sealed partial class MainWindowViewModel : NavViewModelBase
{
    public override IReadOnlyList<NavItem> NavItems { get; } =
    [
        new(Page_HomeLiteral, HomePageName, "Home"),
        new(Page_ModdingLiteral, ModdingPageName, "Wrench"),
        new(Page_ChartingLiteral, ChartingPageName, "Music"),
        new(Page_SettingLiteral, SettingPageName, "Setting")
    ];

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync().ConfigureAwait(true);
        await SettingService.ValidateAsync().ConfigureAwait(true);
#if RELEASE
        await UpdateService.CheckForUpdatesAsync().ConfigureAwait(true);
#endif
        Logger.ZLogInformation($"{nameof(MainWindowViewModel)} Initialized");
    }

    #region Injections

    [UsedImplicitly]
    public required Config Config { get; init; }

    [UsedImplicitly]
    public required LocalizationService LocalizationService { get; init; }

    [UsedImplicitly]
    public required NavigationService NavigationService { get; init; }

    [UsedImplicitly]
    public required ILogger<MainWindowViewModel> Logger { get; init; }

#if RELEASE
    [UsedImplicitly]
    public required IUpdateService UpdateService { get; init; }
#endif
    [UsedImplicitly]
    public required ISettingService SettingService { get; init; }

    #endregion Injections
}