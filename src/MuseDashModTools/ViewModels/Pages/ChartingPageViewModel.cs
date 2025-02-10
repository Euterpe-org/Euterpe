using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;
using WeakReferenceMessenger = CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger;

namespace MuseDashModTools.ViewModels.Pages;

public sealed partial class ChartingPageViewModel : ViewModelBase, IRecipient<string>
{
    private const string Token = "NavigatePanelCharting";

    public static ObservableCollection<PageNavItem> PageNavItems { get; } =
    [
        new("Charts", "", ChartsPanelName, Token) { Selected = true },
        new("Charter", "", CharterPanelName, Token)
    ];

    public NavigationService NavigationService { get; init; } = null!;

    public ChartingPageViewModel()
    {
        WeakReferenceMessenger.Default.Register(this, Token);
    }

    public void Receive(string message)
    {
        switch (message)
        {
            case ChartsPanelName:
                NavigationService.NavigateToPanel<ChartsPanel>(Token);
                break;
            case CharterPanelName:
                NavigationService.NavigateToPanel<CharterPanel>(Token);
                break;
        }
    }

    [RelayCommand]
    private async Task InitializeAsync() => Receive(PageNavItems.FirstOrDefault()?.NavigateKey ?? string.Empty);
}