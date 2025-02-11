using System.Collections.ObjectModel;

namespace MuseDashModTools.ViewModels.Pages;

public sealed partial class ModdingPageViewModel : PageViewModelBase
{
    public override ObservableCollection<NavItem> NavItems { get; } =
    [
        new("Mods", "", ModsPanelName),
        new("Framework", "", FrameworkPanelName),
        new("Develop", "", DevelopPanelName)
    ];

    [RelayCommand]
    protected override void Initialize()
    {
        base.Initialize();
        Logger.ZLogInformation($"ModdingPage Initialized");
    }

    protected override void Navigate(NavItem? value)
    {
        Content = value?.NavigateKey switch
        {
            ModsPanelName => NavigationService.NavigateTo<ModsPanel>(),
            FrameworkPanelName => NavigationService.NavigateTo<FrameworkPanel>(),
            DevelopPanelName => NavigationService.NavigateTo<DevelopPanel>(),
            _ => Content
        };
    }

    #region Injections

    [UsedImplicitly]
    public NavigationService NavigationService { get; init; } = null!;

    [UsedImplicitly]
    public ILogger<ModdingPageViewModel> Logger { get; init; } = null!;

    #endregion Injections
}