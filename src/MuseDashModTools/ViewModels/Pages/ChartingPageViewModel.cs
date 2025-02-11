using System.Collections.ObjectModel;

namespace MuseDashModTools.ViewModels.Pages;

public sealed partial class ChartingPageViewModel : PageViewModelBase
{
    public override ObservableCollection<NavItem> NavItems { get; } =
    [
        new("Charts", "", ChartsPanelName),
        new("Charter", "", CharterPanelName)
    ];

    [RelayCommand]
    protected override void Initialize()
    {
        base.Initialize();
        Logger.ZLogInformation($"ChartingPage Initialized");
    }

    protected override void Navigate(NavItem? value)
    {
        Content = value?.NavigateKey switch
        {
            ChartsPanelName => NavigationService.NavigateTo<ChartsPanel>(),
            CharterPanelName => NavigationService.NavigateTo<CharterPanel>(),
            _ => throw new UnreachableException()
        };
    }

    #region Injections

    [UsedImplicitly]
    public NavigationService NavigationService { get; init; } = null!;

    [UsedImplicitly]
    public ILogger<ChartingPageViewModel> Logger { get; init; } = null!;

    #endregion Injections
}