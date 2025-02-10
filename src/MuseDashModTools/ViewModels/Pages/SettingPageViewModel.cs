using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;
using WeakReferenceMessenger = CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger;

namespace MuseDashModTools.ViewModels.Pages;

public sealed partial class SettingPageViewModel : ViewModelBase, IRecipient<string>
{
    private const string Token = "NavigatePanelSetting";

    public static ObservableCollection<PageNavItem> PageNavItems { get; } =
    [
        new("About", "", AboutPanelName, Token) { Selected = true },
        new("Appearance", "", AppearancePanelName, Token),
        new("Experience", "", ExperiencePanelName, Token),
        new("Download", "", DownloadPanelName, Token),
        new("Advanced", "", AdvancedPanelName, Token)
    ];

    public NavigationService NavigationService { get; init; } = null!;

    public SettingPageViewModel()
    {
        WeakReferenceMessenger.Default.Register(this, Token);
    }

    public void Receive(string message)
    {
        switch (message)
        {
            case AboutPanelName:
                NavigationService.NavigateToPanel<AboutPanel>(Token);
                break;
            case AppearancePanelName:
                NavigationService.NavigateToPanel<AppearancePanel>(Token);
                break;
            case ExperiencePanelName:
                NavigationService.NavigateToPanel<ExperiencePanel>(Token);
                break;
            case DownloadPanelName:
                NavigationService.NavigateToPanel<DownloadPanel>(Token);
                break;
            case AdvancedPanelName:
                NavigationService.NavigateToPanel<AdvancedPanel>(Token);
                break;
        }
    }

    [RelayCommand]
    private async Task InitializeAsync() => Receive(PageNavItems.FirstOrDefault()?.NavigateKey ?? string.Empty);
}