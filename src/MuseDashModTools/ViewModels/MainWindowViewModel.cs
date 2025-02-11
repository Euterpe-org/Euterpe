using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;

namespace MuseDashModTools.ViewModels;

public sealed partial class MainWindowViewModel : ViewModelBase, IRecipient<string>
{
    private const string Token = "NavigatePage";

    [ObservableProperty]
    public partial Control? Content { get; set; }

    public static ObservableCollection<PageNavItem> PageNavItems { get; } =
    [
        new(XAML_Page_Home, "Home", HomePageName, Token),
        new(XAML_Page_Modding, "Wrench", ModdingPageName, Token),
        new(XAML_Page_Charting, "Disc", ChartingPageName, Token),
        new(XAML_Page_Setting, "Setting", SettingPageName, Token)
    ];

    public MainWindowViewModel()
    {
        WeakReferenceMessenger.Default.Register(this, Token);
    }

    [RelayCommand]
    private async Task InitializeAsync()
    {
        await SavingService.LoadSettingAsync().ConfigureAwait(true);
        GetCurrentApplication().RequestedThemeVariant = AvaloniaResources.ThemeVariants[Setting.Theme];
#if RELEASE
        await UpdateService.CheckForUpdatesAsync().ConfigureAwait(true);
#endif
        Logger.ZLogInformation($"MainWindow Initialized");
        Content = NavigationService.NavigateTo<HomePage>();
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