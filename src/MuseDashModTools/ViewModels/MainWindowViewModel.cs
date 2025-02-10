using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;

namespace MuseDashModTools.ViewModels;

public sealed partial class MainWindowViewModel : ViewModelBase, IRecipient<string>
{
    public static ObservableCollection<PageNavItem> PageNavItems { get; } =
    [
        new(XAML_Page_Home, "Home", HomePageName),
        new(XAML_Page_Category_Modding, "Wrench", ModdingCategoryName)
        {
            IsNavigable = false,
            Children =
            [
                new(XAML_Page_ModManage, "GridView", ModManagePageName),
                new(XAML_Page_ModDevelop, "Code", ModDevelopPageName) { Status = "WIP" }
            ]
        },
        new(XAML_Page_Category_Charting, "Disc", ChartingCategoryName)
        {
            IsNavigable = false,
            Children =
            [
                new(XAML_Page_ChartManage, "Song", ChartManagePageName) { Status = "WIP" },
                new(XAML_Page_ChartToolkit, "Briefcase", ChartToolkitPageName) { Status = "WIP" }
            ]
        },
        new(XAML_Page_About, "InfoCircle", AboutPageName),
        new(XAML_Page_Setting, "Setting", SettingPageName)
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