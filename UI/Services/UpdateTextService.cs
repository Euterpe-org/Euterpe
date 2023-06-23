using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Contracts.ViewModels;
using static MuseDashModToolsUI.Localization.Resources;

namespace MuseDashModToolsUI.Services;

public class UpdateTextService : IUpdateTextService
{
    private readonly IMainWindowViewModel _mainWindowViewModel;
    private readonly ISettingsViewModel _settingsViewModel;

    public UpdateTextService(IMainWindowViewModel mainWindowViewModel, ISettingsViewModel settingsViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel;
        _settingsViewModel = settingsViewModel;
    }

    public void ChangeTabName()
    {
        _mainWindowViewModel.Tabs[0].DisplayName = XAML_Tab_ModManage;
        _mainWindowViewModel.Tabs[1].DisplayName = XAML_Tab_Setting;
    }

    public void ChangeOptionName()
    {
        _settingsViewModel.AskTypes = new[] { XAML_AskType_Always, XAML_AskType_Yes, XAML_AskType_No };
    }
}