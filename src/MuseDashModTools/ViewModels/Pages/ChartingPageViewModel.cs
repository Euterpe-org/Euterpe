using System.Collections.ObjectModel;

namespace MuseDashModTools.ViewModels.Pages;

public sealed partial class ChartingPageViewModel : ViewModelBase /*, IRecipient<string>*/
{
    private const string Token = "NavigatePanelCharting";
    public NavigationService NavigationService { get; init; } = null!;

    [ObservableProperty]
    public partial Control? Content { get; set; }

    [ObservableProperty]
    public partial PageNavItem? SelectedItem { get; set; }

    public static ObservableCollection<PageNavItem> PanelNavItems { get; } =
    [
        new("Charts", "", ChartsPanelName, Token),
        new("Charter", "", CharterPanelName, Token)
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
            ChartsPanelName => NavigationService.NavigateTo<ChartsPanel>(),
            CharterPanelName => NavigationService.NavigateTo<CharterPanel>(),
            _ => Content
        };
    }
}