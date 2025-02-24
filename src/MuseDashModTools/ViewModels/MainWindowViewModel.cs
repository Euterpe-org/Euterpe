using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using ReactiveUI;

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

    public MainWindowViewModel()
    {
        this.WhenActivated(async void (CompositeDisposable _) =>
        {
            try
            {
                await InitializeAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                App.Container.Resolve<ILogger<MainWindowViewModel>>().ZLogError(ex, $"{nameof(MainWindowViewModel)} Initialize Error");
            }
        });
    }

    private async Task InitializeAsync()
    {
        Initialize();
        await SavingService.LoadSettingAsync().ConfigureAwait(true);
        GetCurrentApplication().RequestedThemeVariant = AvaloniaResources.ThemeVariants[Config.Theme];
#if RELEASE
        await UpdateService.CheckForUpdatesAsync().ConfigureAwait(true);
#endif
        Logger.ZLogInformation($"MainWindow Initialized");
    }

    #region Injections

    [UsedImplicitly]
    public required IDownloadManager DownloadManager { get; init; }

    [UsedImplicitly]
    public required ILogger<MainWindowViewModel> Logger { get; init; }

    [UsedImplicitly]
    public required ISavingService SavingService { get; init; }

    [UsedImplicitly]
    public required NavigationService NavigationService { get; init; }

    [UsedImplicitly]
    public required IUpdateService UpdateService { get; init; }

    [UsedImplicitly]
    public required Config Config { get; init; }

    #endregion Injections
}