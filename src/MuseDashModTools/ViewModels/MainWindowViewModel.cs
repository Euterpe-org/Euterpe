using System.Collections.ObjectModel;

namespace MuseDashModTools.ViewModels;

public sealed partial class MainWindowViewModel : NavViewModelBase
{
    public override ObservableCollection<NavItem> NavItems { get; } =
    [
        new(XAML_Page_Home, HomePageName, "Home"),
        new(XAML_Page_Modding, ModdingPageName, "Wrench"),
        new(XAML_Page_Charting, ChartingPageName, "Disc"),
        new(XAML_Page_Setting, SettingPageName, "Setting")
    ];

    [RelayCommand]
    private async Task InitializeAsync()
    {
        Initialize();
        await SavingService.LoadSettingAsync().ConfigureAwait(true);
        GetCurrentApplication().RequestedThemeVariant = AvaloniaResources.ThemeVariants[Setting.Theme];
#if RELEASE
        await UpdateService.CheckForUpdatesAsync().ConfigureAwait(true);
#endif
        Logger.ZLogInformation($"MainWindow Initialized");
    }

    #region Injections

    [UsedImplicitly]
    public IDownloadManager DownloadManager { get; init; } = null!;

    [UsedImplicitly]
    public ILogger<MainWindowViewModel> Logger { get; init; } = null!;

    [UsedImplicitly]
    public ISavingService SavingService { get; init; } = null!;

    [UsedImplicitly]
    public NavigationService NavigationService { get; init; } = null!;

    [UsedImplicitly]
    public IUpdateService UpdateService { get; init; } = null!;

    [UsedImplicitly]
    public Setting Setting { get; init; } = null!;

    #endregion Injections
}