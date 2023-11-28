namespace MuseDashModToolsUI.Services;

public partial class UpdateUIService
{
    /// <summary>
    ///     Change Setting Window select option name
    /// </summary>
    private void ChangeOptionName()
    {
        SettingsViewModel.Value.AskTypes = [XAML_AskType_Always, XAML_AskType_Yes, XAML_AskType_No];
        SettingsViewModel.Value.DownloadSources =
            [XAML_DownloadSource_Github, XAML_DownloadSource_GithubMirror, XAML_DownloadSource_Gitee, XAML_DownloadSource_Custom];
        ChartDownloadViewModel.Value.SortOptions =
        [
            XAML_ChartSortOption_Default, XAML_ChartSortOption_Name,
            XAML_ChartSortOption_Downloads, XAML_ChartSortOption_Likes,
            XAML_ChartSortOption_Level, XAML_ChartSortOption_Latest
        ];
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