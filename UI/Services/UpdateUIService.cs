#pragma warning disable CS8618

namespace MuseDashModToolsUI.Services;

public partial class UpdateUIService : IUpdateUIService
{
    public ILogger Logger { get; init; }

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

    public async Task InitializeTabs()
    {
        SettingsViewModel.Value.UpdatePath();
        await ModManageViewModel.Value.Initialize();
        await LogAnalysisViewModel.Value.Initialize();

        Logger.Information("Tabs initialized");
    }
}