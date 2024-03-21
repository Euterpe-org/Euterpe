using Avalonia.Styling;

#pragma warning disable CS8618

namespace MuseDashModToolsUI.Services;

public sealed partial class UpdateUIService : IUpdateUIService
{
    public void UpdateText()
    {
        ChangeOptionName();
        RecoverOption();
    }

    public void ChangeTheme(string themeName)
    {
        var app = Application.Current;
        if (app is null)
        {
            return;
        }

        app.RequestedThemeVariant = themeName switch
        {
            "Dark" => ThemeVariant.Dark,
            "Light" => ThemeVariant.Light,
            _ => app.RequestedThemeVariant
        };

        Settings.Theme = themeName;
    }

    public async Task InitializeAllTabsAsync()
    {
        SettingsViewModel.Value.Initialize();
        await ModManageViewModel.Value.Initialize();
        await ChartDownloadViewModel.Value.Initialize();
        await LogAnalysisViewModel.Value.Initialize();
        InfoJsonViewModel.Value.Initialize();

        Logger.Information("All Tabs initialized");
    }

    public async Task InitializeTabsOnChoosePathAsync()
    {
        await ModManageViewModel.Value.Initialize();
        await LogAnalysisViewModel.Value.Initialize();

        Logger.Information("Path changed, ModManage and LogAnalysis tabs initialized");
    }

    #region Services

    [UsedImplicitly]
    public ILogger Logger { get; init; }

    [UsedImplicitly]
    public Lazy<IChartDownloadViewModel> ChartDownloadViewModel { get; init; }

    [UsedImplicitly]
    public Lazy<IInfoJsonViewModel> InfoJsonViewModel { get; init; }

    [UsedImplicitly]
    public Lazy<ILogAnalysisViewModel> LogAnalysisViewModel { get; init; }

    [UsedImplicitly]
    public Lazy<IMainWindowViewModel> MainWindowViewModel { get; init; }

    [UsedImplicitly]
    public Lazy<IModManageViewModel> ModManageViewModel { get; init; }

    [UsedImplicitly]
    public Lazy<ISettingsViewModel> SettingsViewModel { get; init; }

    [UsedImplicitly]
    public Setting Settings { get; init; }

    #endregion
}