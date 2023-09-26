#pragma warning disable CS8618

namespace MuseDashModToolsUI.Services;

public partial class UpdateUIService : IUpdateUIService
{
    [UsedImplicitly]
    public ILogAnalysisViewModel LogAnalysisViewModel { get; init; }

    [UsedImplicitly]
    public IMainWindowViewModel MainWindowViewModel { get; init; }

    [UsedImplicitly]
    public IModManageViewModel ModManageViewModel { get; init; }

    [UsedImplicitly]
    public ISettingsViewModel SettingsViewModel { get; init; }

    [UsedImplicitly]
    public ISavingService SavingService { get; init; }

    public void UpdateText()
    {
        ChangeTabName();
        ChangeOptionName();
        RecoverOption();
    }

    public async Task InitializeTabs()
    {
        SettingsViewModel.UpdatePath();
        await ModManageViewModel.Initialize();
        await LogAnalysisViewModel.Initialize();
    }
}