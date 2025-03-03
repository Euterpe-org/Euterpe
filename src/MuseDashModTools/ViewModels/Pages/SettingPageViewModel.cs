namespace MuseDashModTools.ViewModels.Pages;

public sealed partial class SettingPageViewModel : NavViewModelBase
{
    public override IReadOnlyList<NavItem> NavItems { get; } =
    [
        new(XAML_Panel_Setting_About, AboutPanelName),
        new(XAML_Panel_Setting_Appearance, AppearancePanelName),
        new(XAML_Panel_Setting_Experience, ExperiencePanelName),
        new(XAML_Panel_Setting_FileManagement, FileManagementPanelName),
        new(XAML_Panel_Setting_Download, DownloadPanelName),
        new(XAML_Panel_Setting_Advanced, AdvancedPanelName)
    ];

    [RelayCommand]
    protected override Task InitializeAsync()
    {
        base.InitializeAsync();

        Logger.ZLogInformation($"{nameof(SettingPageViewModel)} Initialized");
        return Task.CompletedTask;
    }

    #region Injections

    [UsedImplicitly]
    public required ILogger<SettingPageViewModel> Logger { get; init; }

    [UsedImplicitly]
    public required NavigationService NavigationService { get; init; }

    #endregion Injections
}