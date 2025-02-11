using System.Collections.ObjectModel;

namespace MuseDashModTools.ViewModels.Pages;

public sealed partial class SettingPageViewModel : ViewModelBase
{
    private const string Token = "NavigatePanelSetting";
    public NavigationService NavigationService { get; init; } = null!;

    [ObservableProperty]
    public partial Control? Content { get; set; }

    [ObservableProperty]
    public partial PageNavItem? SelectedItem { get; set; }

    public static ObservableCollection<PageNavItem> PanelNavItems { get; } =
    [
        new("About", "", AboutPanelName, Token),
        new("Appearance", "", AppearancePanelName, Token),
        new("Experience", "", ExperiencePanelName, Token),
        new("Download", "", DownloadPanelName, Token),
        new("Advanced", "", AdvancedPanelName, Token)
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
            AboutPanelName => NavigationService.NavigateTo<AboutPanel>(),
            AppearancePanelName => NavigationService.NavigateTo<AppearancePanel>(),
            ExperiencePanelName => NavigationService.NavigateTo<ExperiencePanel>(),
            DownloadPanelName => NavigationService.NavigateTo<DownloadPanel>(),
            AdvancedPanelName => NavigationService.NavigateTo<AdvancedPanel>(),
            _ => Content
        };
    }
}