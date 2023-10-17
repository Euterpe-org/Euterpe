namespace MuseDashModToolsUI.Services;

public partial class UpdateUIService
{
    /// <summary>
    ///     Change Main Window tab display name
    /// </summary>
    private void ChangeTabName()
    {
        MainWindowViewModel.Value.Tabs[0].DisplayName = XAML_Tab_ModManage;
        MainWindowViewModel.Value.Tabs[1].DisplayName = XAML_Tab_ChartDownload;
        MainWindowViewModel.Value.Tabs[2].DisplayName = XAML_Tab_LogAnalysis;
        MainWindowViewModel.Value.Tabs[3].DisplayName = XAML_Tab_Setting;
        MainWindowViewModel.Value.Tabs[4].DisplayName = XAML_Tab_About;
    }

    /// <summary>
    ///     Change Setting Window select option name
    /// </summary>
    private void ChangeOptionName()
    {
        SettingsViewModel.Value.AskTypes = new[] { XAML_AskType_Always, XAML_AskType_Yes, XAML_AskType_No };
        SettingsViewModel.Value.DownloadSources = new[]
            { XAML_DownloadSource_Github, XAML_DownloadSource_GithubMirror, XAML_DownloadSource_Gitee, XAML_DownloadSource_Custom };
        ChartDownloadViewModel.Value.SortOptions = new[]
        {
            XAML_ChartSortOption_Default, XAML_ChartSortOption_Name,
            XAML_ChartSortOption_Downloads, XAML_ChartSortOption_Likes,
            XAML_ChartSortOption_Level, XAML_ChartSortOption_Latest
        };
    }

    /// <summary>
    ///     Recover Setting Window option index
    /// </summary>
    private void RecoverOption()
    {
        SettingsViewModel.Value.CurrentDownloadSource = (int)SavingService.Value.Settings.DownloadSource;
        SettingsViewModel.Value.DisableDependenciesWhenDeleting = (int)SavingService.Value.Settings.AskDisableDependenciesWhenDeleting;
        SettingsViewModel.Value.DisableDependenciesWhenDisabling = (int)SavingService.Value.Settings.AskDisableDependenciesWhenDisabling;
        SettingsViewModel.Value.EnableDependenciesWhenEnabling = (int)SavingService.Value.Settings.AskEnableDependenciesWhenEnabling;
        SettingsViewModel.Value.EnableDependenciesWhenInstalling = (int)SavingService.Value.Settings.AskEnableDependenciesWhenInstalling;
        ChartDownloadViewModel.Value.CurrentSortOptionIndex = (int)ChartDownloadViewModel.Value.SortOption;
    }
}