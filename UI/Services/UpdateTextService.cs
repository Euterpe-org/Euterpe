using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Contracts.ViewModels;
using static MuseDashModToolsUI.Localization.Resources;

#pragma warning disable CS8618

namespace MuseDashModToolsUI.Services;

public class UpdateTextService : IUpdateTextService
{
    public IMainWindowViewModel MainWindowViewModel { get; init; }
    public ISettingsViewModel SettingsViewModel { get; init; }
    public ISavingService SavingService { get; init; }

    public void UpdateText()
    {
        ChangeTabName();
        ChangeOptionName();
    }

    private void ChangeTabName()
    {
        MainWindowViewModel.Tabs[0].DisplayName = XAML_Tab_ModManage;
        MainWindowViewModel.Tabs[1].DisplayName = XAML_Tab_LogAnalysis;
        MainWindowViewModel.Tabs[2].DisplayName = XAML_Tab_Setting;
    }

    private void ChangeOptionName()
    {
        SettingsViewModel.AskTypes = new[] { XAML_AskType_Always, XAML_AskType_Yes, XAML_AskType_No };
        SettingsViewModel.DownloadSources = new[] { XAML_DownloadSource_Github, XAML_DownloadSource_GithubMirror, XAML_DownloadSource_Gitee };
        SettingsViewModel.CurrentDownloadSource = (int)SavingService.Settings.DownloadSource;
        SettingsViewModel.DisableDependenciesWhenDeleting = (int)SavingService.Settings.AskDisableDependenciesWhenDeleting;
        SettingsViewModel.DisableDependenciesWhenDisabling = (int)SavingService.Settings.AskDisableDependenciesWhenDisabling;
        SettingsViewModel.EnableDependenciesWhenEnabling = (int)SavingService.Settings.AskEnableDependenciesWhenEnabling;
        SettingsViewModel.EnableDependenciesWhenInstalling = (int)SavingService.Settings.AskEnableDependenciesWhenInstalling;
    }
}