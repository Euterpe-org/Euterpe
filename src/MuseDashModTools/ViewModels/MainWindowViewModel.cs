using System.Collections.ObjectModel;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.Messaging;
using static MuseDashModTools.Models.Constants.PageNames;

namespace MuseDashModTools.ViewModels;

public sealed partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool _isCollapsed;

    public static string Version => $"v{AppVersion}";

    public static ObservableCollection<PageNavItem> PageNavItems =>
    [
        new(HomePageName),
        new("Modding")
        {
            IsNavigable = false,
            Children =
            [
                ModManagePageName,
                ModDevelopPageName
            ]
        },
        new("Charting")
        {
            IsNavigable = false,
            Children =
            [
                ChartManagePageName,
                ChartToolkitPageName
            ]
        },
        new(AboutPageName),
        new(SettingPageName)
    ];

    public MainWindowViewModel()
    {
        WeakReferenceMessenger.Default.Register<MainWindowViewModel, string>(this, OnNavigation);
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

    private void OnNavigation(MainWindowViewModel vm, string pageName)
    {
        switch (pageName)
        {
            case HomePageName:
                NavigationService.NavigateToView<HomePage>();
                break;
            case ModManagePageName:
                NavigationService.NavigateToView<ModManagePage>();
                break;
            /*case PageNames.ModDevelopPage:
                NavigationService.NavigateToView<ModDevelopPage>();
                break;*/
            case ChartManagePageName:
                NavigationService.NavigateToView<ChartManagePage>();
                break;
            /*case PageNames.ChartToolkitPage:
                NavigationService.NavigateToView<ChartToolkitPage>();
                break;*/
            case AboutPageName:
                NavigationService.NavigateToView<AboutPage>();
                break;
            case SettingPageName:
                NavigationService.NavigateToView<SettingPage>();
                break;
        }
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