using System.Globalization;
using Autofac;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace MuseDashModToolsUI.ViewModels;

public sealed partial class MainWindowViewModel : ViewModelBase, IMainWindowViewModel
{
    private readonly ILogger _logger;
    private readonly ISavingService _savingService;
    private readonly Setting _settings;
    [ObservableProperty] private ViewModelBase _content;
    [ObservableProperty] private int _selectedTabIndex;
    [ObservableProperty] private List<TabView> _tabs = [];

    [UsedImplicitly]
    public IUpdateUIService UpdateUIService { get; init; }

    public static string AppVersion => BuildInfo.AppVersion;

    public MainWindowViewModel(IComponentContext context)
    {
        _logger = context.Resolve<ILogger>();
        _savingService = context.Resolve<ISavingService>();

        _savingService.LoadSettings();
        _settings = context.Resolve<Setting>();
        if (!_settings.LanguageCode.IsNullOrEmpty())
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(_settings.LanguageCode!);
        }
#if !DEBUG
        context.Resolve<IDownloadService>().CheckUpdatesAsync();
#endif
        _savingService.InitializeSettingsAsync().ConfigureAwait(false);
        _logger.Information("Main Window initialized");
        AppDomain.CurrentDomain.ProcessExit += OnExit;
    }

    [RelayCommand]
    private void OnChangeTheme()
    {
        var targetTheme = _settings.Theme == "Dark" ? "Light" : "Dark";
        UpdateUIService.ChangeTheme(targetTheme);
        _logger.Information("Change Theme to {Theme}", targetTheme);
    }

    private async void OnExit(object? sender, EventArgs e) => await _savingService.SaveAsync();
}