using System.Collections.ObjectModel;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.Messaging;

namespace MuseDashModTools.ViewModels;

public sealed partial class MainWindowViewModel : ViewModelBase, IRecipient<string>
{
    [ObservableProperty]
    private bool _isCollapsed;

    public static string Version => $"v{AppVersion}";

    public static ObservableCollection<PageNavItem> PageNavItems =>
    [
        new(XAML_Page_Home, HomePageName),
        new(XAML_Page_Category_Modding, ModdingCategoryName)
        {
            IsNavigable = false,
            Children =
            [
                new PageNavItem(XAML_Page_ModManage, ModManagePageName),
                new PageNavItem(XAML_Page_ModDevelop, ModDevelopPageName)
            ]
        },
        new(XAML_Page_Category_Charting, ChartingCategoryName)
        {
            IsNavigable = false,
            Children =
            [
                new PageNavItem(XAML_Page_ChartManage, ChartManagePageName),
                new PageNavItem(XAML_Page_ChartToolkit, ChartToolkitPageName)
            ]
        },
        new(XAML_Page_About, AboutPageName),
        new(XAML_Page_Setting, SettingPageName)
    ];

    public MainWindowViewModel()
    {
        WeakReferenceMessenger.Default.Register(this, "NavigatePage");
    }

    [RelayCommand]
    private async Task InitializeAsync()
    {
        await SavingService.LoadSettingAsync().ConfigureAwait(true);
        if (Setting.Theme == "Light")
        {
            GetCurrentApplication().RequestedThemeVariant = ThemeVariant.Light;
        }
#if !DEBUG
        await DownloadManager.CheckForUpdatesAsync().ConfigureAwait(true);
#endif
        GetCurrentDesktop().Exit += (_, _) => SavingService.SaveSettingAsync();
        Logger.Information("MainWindow Initialized");
    }

    #region Injections

    [UsedImplicitly]
    public IDownloadManager DownloadManager { get; init; } = null!;

    [UsedImplicitly]
    public ILogger Logger { get; init; } = null!;

    [UsedImplicitly]
    public INavigationService NavigationService { get; init; } = null!;

    [UsedImplicitly]
    public ISavingService SavingService { get; init; } = null!;

    [UsedImplicitly]
    public Setting Setting { get; init; } = null!;

    #endregion Injections
}