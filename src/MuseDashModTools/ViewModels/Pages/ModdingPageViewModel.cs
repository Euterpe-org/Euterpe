using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;

namespace MuseDashModTools.ViewModels.Pages;

public sealed partial class ModdingPageViewModel : ViewModelBase, IRecipient<string>
{
    private const string Token = "NavigatePanelModding";

    public static ObservableCollection<PageNavItem> PageNavItems { get; } =
    [
        new("Mods", "", ModsPanelName, Token),
        new("Framework", "", FrameworkPanelName, Token),
        new("Develop", "", DevelopPanelName, Token)
    ];

    public NavigationService NavigationService { get; init; } = null!;

    public ModdingPageViewModel()
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
            case ModsPanelName:
                NavigationService.NavigateToPanel<ModsPanel>(Token);
                break;
            case FrameworkPanelName:
                NavigationService.NavigateToPanel<FrameworkPanel>(Token);
                break;
            case DevelopPanelName:
                NavigationService.NavigateToPanel<DevelopPanel>(Token);
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