using System.Collections.ObjectModel;

namespace MuseDashModTools.ViewModels.Pages;

public sealed partial class ModdingPageViewModel : ViewModelBase /*, IRecipient<string>*/
{
    private const string Token = "NavigatePanelModding";
    public NavigationService NavigationService { get; init; } = null!;
    public ILogger<ModdingPageViewModel> Logger { get; init; } = null!;

    [ObservableProperty]
    public partial Control? Content { get; set; }

    [ObservableProperty]
    public partial PageNavItem? SelectedItem { get; set; }

    public static ObservableCollection<PageNavItem> PanelNavItems { get; } =
    [
        new("Mods", "", ModsPanelName, Token),
        new("Framework", "", FrameworkPanelName, Token),
        new("Develop", "", DevelopPanelName, Token)
    ];

    [RelayCommand]
    private void Initialize()
    {
        SelectedItem = PanelNavItems[0];
    }

    partial void OnSelectedItemChanged(PageNavItem? value)
    {
        Content = value?.NavigateKey switch
        {
            ModsPanelName => NavigationService.NavigateTo<ModsPanel>(),
            FrameworkPanelName => NavigationService.NavigateTo<FrameworkPanel>(),
            DevelopPanelName => NavigationService.NavigateTo<DevelopPanel>(),
            _ => Content
        };
    }
}