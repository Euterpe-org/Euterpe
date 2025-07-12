namespace MuseDashModTools.ViewModels.Pages;

public sealed partial class SettingPageViewModel : NavViewModelBase
{
    public override IReadOnlyList<NavItem> NavItems { get; } =
    [
        new(Panel_Setting_About, AboutPanelName),
        new(Panel_Setting_Appearance, AppearancePanelName),
        new(Panel_Setting_Experience, ExperiencePanelName),
        new(Panel_Setting_FileManagement, FileManagementPanelName),
        new(Panel_Setting_Download, DownloadPanelName),
        new(Panel_Setting_Advanced, AdvancedPanelName)
    ];

    #region Injections

    [UsedImplicitly]
    public required ILogger<SettingPageViewModel> Logger { get; init; }

    #endregion Injections

    public override Task InitializeAsync()
    {
        base.InitializeAsync();

        Logger.ZLogInformation($"{nameof(SettingPageViewModel)} Initialized");
        return Task.CompletedTask;
    }
}