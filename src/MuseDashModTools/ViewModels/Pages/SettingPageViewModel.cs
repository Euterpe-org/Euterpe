using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;
using WeakReferenceMessenger = CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger;

namespace MuseDashModTools.ViewModels.Pages;

public sealed partial class SettingPageViewModel : ViewModelBase, IRecipient<string>
{
    private const string Token = "NavigatePanelSetting";

    public static ObservableCollection<PageNavItem> PageNavItems { get; } =
    [
        new("About", "", AboutPanelName, Token),
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
        foreach (var item in PageNavItems.Where(x => x.Selected))
        {
            item.Selected = false;
        }

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

        var newItem = PageNavItems.FirstOrDefault(x => x.NavigateKey == message);
        if (newItem != null)
        {
            newItem.Selected = true;
        }
    }

    [RelayCommand]
    private async Task InitializeAsync()
    {
        if (PageNavItems.Any(x => x.Selected))
        {
            return;
        }

        var firstItem = PageNavItems.FirstOrDefault();
        if (firstItem == null)
        {
            return;
        }

        Receive(firstItem.NavigateKey);
        firstItem.Selected = true;
    }
}