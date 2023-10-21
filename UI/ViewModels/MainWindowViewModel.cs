using System.Globalization;
using Autofac;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace MuseDashModToolsUI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, IMainWindowViewModel
{
    private readonly ILocalService _localService;
    private readonly ILogger _logger;
    private readonly ISavingService _savingService;
    [ObservableProperty] private ViewModelBase _content;
    [ObservableProperty] private int _selectedTabIndex;
    [ObservableProperty] private List<TabView> _tabs = new();
    public static string Version => BuildInfo.Version;

    public MainWindowViewModel(IComponentContext context)
    {
        _localService = context.Resolve<ILocalService>();
        _logger = context.Resolve<ILogger>();
        _savingService = context.Resolve<ISavingService>();

        if (_savingService.Settings.LanguageCode is not null)
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(_savingService.Settings.LanguageCode);

        Tabs = new List<TabView>
        {
            new((ViewModelBase)context.Resolve<IModManageViewModel>(), XAML_Tab_ModManage, "ModManage"),
            new((ViewModelBase)context.Resolve<IChartDownloadViewModel>(), XAML_Tab_ChartDownload, "ChartDownload"),
            new((ViewModelBase)context.Resolve<ILogAnalysisViewModel>(), XAML_Tab_LogAnalysis, "LogAnalysis"),
            new((ViewModelBase)context.Resolve<ISettingsViewModel>(), XAML_Tab_Setting, "Setting"),
            new((ViewModelBase)context.Resolve<IAboutViewModel>(), XAML_Tab_About, "About")
        };
        SwitchTab();
#if !DEBUG
        context.Resolve<IGitHubService>().CheckUpdates();
#endif
        _savingService.InitializeSettingsAsync().ConfigureAwait(false);
        _logger.Information("Main Window initialized");
        AppDomain.CurrentDomain.ProcessExit += OnExit!;
    }

    [RelayCommand]
    private void SwitchTab()
    {
        Content = Tabs[SelectedTabIndex].ViewModel;
        var name = Tabs[SelectedTabIndex].Name;
        _logger.Information("Switching tab to {Name}", name);
    }

    [RelayCommand]
    private void OnLaunchVanillaGameAsync() => _localService.OnLaunchGame(false);

    [RelayCommand]
    private void OnLaunchModdedGameAsync() => _localService.OnLaunchGame(true);

    private void OnExit(object sender, EventArgs e) => _savingService.SaveAsync().Wait();
}