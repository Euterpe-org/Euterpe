namespace MuseDashModTools.ViewModels;

public sealed partial class MainWindowViewModel : NavViewModelBase
{
    public override IReadOnlyList<NavItem> NavItems { get; } =
    [
        new(XAML_Page_Home, HomePageName, "Home"),
        new(XAML_Page_Modding, ModdingPageName, "Wrench"),
        new(XAML_Page_Charting, ChartingPageName, "Music"),
        new(XAML_Page_Setting, SettingPageName, "Setting")
    ];

    [RelayCommand]
    protected override async Task InitializeAsync()
    {
        await base.InitializeAsync().ConfigureAwait(true);
        await SettingService.LoadAsync().ConfigureAwait(true);
        GetCurrentApplication().RequestedThemeVariant = AvaloniaResources.ThemeVariants[Config.Theme];
#if RELEASE
        await UpdateService.CheckForUpdatesAsync().ConfigureAwait(true);
#endif
        Logger.ZLogInformation($"{nameof(MainWindowViewModel)} Initialized");
    }

    #region Injections

    [UsedImplicitly]
    public required Config Config { get; init; }

    [UsedImplicitly]
    public required NavigationService NavigationService { get; init; }

    [UsedImplicitly]
    public required ILogger<MainWindowViewModel> Logger { get; init; }

    [UsedImplicitly]
    public required ISettingService SettingService { get; init; }

#if RELEASE
    [UsedImplicitly]
    public required IUpdateService UpdateService { get; init; }
#endif

    #endregion Injections
}