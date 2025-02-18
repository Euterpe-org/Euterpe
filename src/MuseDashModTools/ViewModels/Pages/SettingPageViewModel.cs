using System.Collections.ObjectModel;

namespace MuseDashModTools.ViewModels.Pages;

public sealed partial class SettingPageViewModel : NavViewModelBase
{
    public override ObservableCollection<NavItem> NavItems { get; } =
    [
        new("About", AboutPanelName),
        new("Appearance", AppearancePanelName),
        new("Experience", ExperiencePanelName),
        new("File Management", FileManagementPanelName),
        new("Download", DownloadPanelName),
        new("Advanced", AdvancedPanelName)
    ];

    [RelayCommand]
    protected override void Initialize()
    {
        base.Initialize();
        Logger.ZLogInformation($"SettingPage Initialized");
    }

    #region Injections

    [UsedImplicitly]
    public ILogger<SettingPageViewModel> Logger { get; init; } = null!;

    [UsedImplicitly]
    public NavigationService NavigationService { get; init; } = null!;

    #endregion Injections
}