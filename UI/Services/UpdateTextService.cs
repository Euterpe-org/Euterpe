using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Contracts.ViewModels;

#pragma warning disable CS8618

namespace MuseDashModToolsUI.Services;

public partial class UpdateTextService : IUpdateTextService
{
    public IMainWindowViewModel MainWindowViewModel { get; init; }
    public ISettingsViewModel SettingsViewModel { get; init; }
    public ISavingService SavingService { get; init; }

    public void UpdateText()
    {
        ChangeTabName();
        ChangeOptionName();
        RecoverOption();
    }
}