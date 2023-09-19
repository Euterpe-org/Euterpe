#pragma warning disable CS8618

namespace MuseDashModToolsUI.Services;

public partial class UpdateTextService : IUpdateTextService
{
    [UsedImplicitly]
    public IMainWindowViewModel MainWindowViewModel { get; init; }

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
}