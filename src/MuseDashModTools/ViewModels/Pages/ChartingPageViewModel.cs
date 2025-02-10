using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;
using WeakReferenceMessenger = CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger;

namespace MuseDashModTools.ViewModels.Pages;

public sealed partial class ChartingPageViewModel : ViewModelBase, IRecipient<string>
{
    private const string Token = "NavigatePanelCharting";

    public static ObservableCollection<PageNavItem> PageNavItems { get; } =
    [
        new("Charts", "", ChartsPanelName, Token),
        new("Charter", "", CharterPanelName, Token)
    ];

    public NavigationService NavigationService { get; init; } = null!;

    public ChartingPageViewModel()
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
            case ChartsPanelName:
                NavigationService.NavigateToPanel<ChartsPanel>(Token);
                break;
            case CharterPanelName:
                NavigationService.NavigateToPanel<CharterPanel>(Token);
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