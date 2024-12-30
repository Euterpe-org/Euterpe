using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;

namespace MuseDashModTools.ViewModels;

public sealed partial class MainWindowViewModel : ViewModelBase, IRecipient<string>
{
    [ObservableProperty]
    public partial bool IsCollapsed { get; set; }

    public static ObservableCollection<PageNavItem> PageNavItems { get; } =
    [
        new(XAML_Page_Home, HomePageName),
        new(XAML_Page_Category_Modding, ModdingCategoryName)
        {
            IsNavigable = false,
            Children =
            [
                new PageNavItem(XAML_Page_ModManage, ModManagePageName),
                new PageNavItem(XAML_Page_ModDevelop, ModDevelopPageName) { Status = "WIP" }
            ]
        },
        new(XAML_Page_Category_Charting, ChartingCategoryName)
        {
            IsNavigable = false,
            Children =
            [
                new PageNavItem(XAML_Page_ChartManage, ChartManagePageName) { Status = "WIP" },
                new PageNavItem(XAML_Page_ChartToolkit, ChartToolkitPageName) { Status = "WIP" }
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
        GetCurrentApplication().RequestedThemeVariant = AvaloniaResources.ThemeVariants[Setting.Theme];
#if RELEASE
        await UpdateService.CheckForUpdatesAsync().ConfigureAwait(true);
#endif
        GetCurrentDesktop().Exit += async (_, _) => await OnExitAsync().ConfigureAwait(false);
        Logger.ZLogInformation($"MainWindow Initialized");
    }

    private Task OnExitAsync()
    {
        Setting.Theme = AvaloniaResources.ThemeVariants[GetCurrentApplication().ActualThemeVariant];
        return SavingService.SaveSettingAsync();
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