namespace MuseDashModTools.ViewModels.Pages;

public sealed partial class SettingPageViewModel : NavViewModelBase
{
    public override IReadOnlyList<NavItem> NavItems { get; } =
    [
        new(Panel_Setting_AboutLiteral, AboutPanelName),
        new(Panel_Setting_AppearanceLiteral, AppearancePanelName),
        new(Panel_Setting_ExperienceLiteral, ExperiencePanelName),
        new(Panel_Setting_FileManagementLiteral, FileManagementPanelName),
        new(Panel_Setting_DownloadLiteral, DownloadPanelName),
        new(Panel_Setting_AdvancedLiteral, AdvancedPanelName)
    ];

    public override Task InitializeAsync()
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