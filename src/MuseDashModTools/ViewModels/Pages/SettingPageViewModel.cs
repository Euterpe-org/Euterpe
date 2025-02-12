using System.Collections.ObjectModel;

namespace MuseDashModTools.ViewModels.Pages;

public sealed partial class SettingPageViewModel : PageViewModelBase
{
    public override ObservableCollection<NavItem> NavItems { get; } =
    [
        new("About", AboutPanelName),
        new("Appearance", AppearancePanelName),
        new("Experience", ExperiencePanelName),
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
    public NavigationService NavigationService { get; init; } = null!;

    [UsedImplicitly]
    public ILogger<SettingPageViewModel> Logger { get; init; } = null!;

    #endregion Injections
}