using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;

namespace MuseDashModTools.ViewModels.Pages;

public sealed class ModdingPageViewModel : ViewModelBase, IRecipient<string>
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
    }
}