using System.Collections.ObjectModel;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.Messaging;

namespace MuseDashModTools.ViewModels;

public sealed partial class MainWindowViewModel : ViewModelBase, IRecipient<SelectedPageChangedMessage>
{
    [ObservableProperty]
    private bool _isCollapsed;

    public static string Version => $"v{AppVersion}";

    public static ObservableCollection<PageNavItem> PageNavItems =>
    [
        new(HomePageName),
        new(ModdingCategoryName)
        {
            IsNavigable = false,
            Children =
            [
                ModManagePageName,
                ModDevelopPageName
            ]
        },
        new(ChartingCategoryName)
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
        WeakReferenceMessenger.Default.Register(this);
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