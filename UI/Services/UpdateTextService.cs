using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Contracts.ViewModels;
using static MuseDashModToolsUI.Localization.Resources;

namespace MuseDashModToolsUI.Services;

public class UpdateTextService : IUpdateTextService
{
    public IMainWindowViewModel? MainWindowViewModel { get; init; }
    public ISettingsViewModel? SettingsViewModel { get; init; }


    public void ChangeTabName()
    {
        MainWindowViewModel!.Tabs[0].DisplayName = XAML_Tab_ModManage;
        MainWindowViewModel!.Tabs[1].DisplayName = XAML_Tab_Setting;
    }

    public void ChangeOptionName()
    {
        SettingsViewModel!.AskTypes = new[] { XAML_AskType_Always, XAML_AskType_Yes, XAML_AskType_No };
    }
}