using CommunityToolkit.Mvvm.Messaging;

namespace MuseDashModTools.Models;

public sealed partial class PageNavItem(string displayName, string iconResourceKey, string navigateKey) : ObservableObject
{
    public PageNavItem[] Children { get; init; } = [];
    public string DisplayName { get; set; } = displayName;
    public string IconResourceKey { get; set; } = iconResourceKey;
    public string NavigateKey { get; init; } = navigateKey;
    public string? Status { get; init; }
    public bool IsNavigable { get; init; } = true;
    public bool IsSeparator { get; init; }

    [RelayCommand]
    private void Navigation()
    {
        if (!IsNavigable || IsSeparator)
        {
            return;
        }

        WeakReferenceMessenger.Default.Send(NavigateKey, "NavigatePage");
    }
}