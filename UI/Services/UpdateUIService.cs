#pragma warning disable CS8618

namespace MuseDashModToolsUI.Services;

public partial class UpdateUIService : IUpdateUIService
{
    [UsedImplicitly]
    public ILogger Logger { get; init; }

    [UsedImplicitly]
    public Lazy<IChartDownloadViewModel> ChartDownloadViewModel { get; init; }

    [UsedImplicitly]
    public Lazy<ILogAnalysisViewModel> LogAnalysisViewModel { get; init; }

    [UsedImplicitly]
    public Lazy<IMainWindowViewModel> MainWindowViewModel { get; init; }

    [UsedImplicitly]
    public Lazy<IModManageViewModel> ModManageViewModel { get; init; }

    [UsedImplicitly]
    public Lazy<ISettingsViewModel> SettingsViewModel { get; init; }

    [UsedImplicitly]
    public Lazy<ISavingService> SavingService { get; init; }

    public void UpdateText()
    {
        ChangeTabName();
        ChangeOptionName();
        RecoverOption();
    }

    public async Task InitializeAllTabs()
    {
        SettingsViewModel.Value.Initialize();
        await ModManageViewModel.Value.Initialize();
        await ChartDownloadViewModel.Value.Initialize();
        await LogAnalysisViewModel.Value.Initialize();

        Logger.Information("All Tabs initialized");
    }

    public async Task InitializeTabsOnChoosePath()
    {
        await ModManageViewModel.Value.Initialize();
        await LogAnalysisViewModel.Value.Initialize();

        Logger.Information("Path changed, ModManage and LogAnalysis tabs initialized");
    }
}