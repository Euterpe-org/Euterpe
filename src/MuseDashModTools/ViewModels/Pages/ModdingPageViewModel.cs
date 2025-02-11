using System.Collections.ObjectModel;

namespace MuseDashModTools.ViewModels.Pages;

public sealed partial class ModdingPageViewModel : ViewModelBase /*, IRecipient<string>*/
{
    public NavigationService NavigationService { get; init; } = null!;
    public ILogger<ModdingPageViewModel> Logger { get; init; } = null!;

    [ObservableProperty]
    public partial Control? Content { get; set; }

    [ObservableProperty]
    public partial PageNavItem? SelectedItem { get; set; }

    public static ObservableCollection<PageNavItem> PanelNavItems { get; } =
    [
        new("Mods", "", ModsPanelName),
        new("Framework", "", FrameworkPanelName),
        new("Develop", "", DevelopPanelName)
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