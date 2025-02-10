using CommunityToolkit.Mvvm.Messaging;

namespace MuseDashModTools.Models;

public sealed partial class PageNavItem(string displayName, string iconResourceKey, string navigateKey, string token) : ObservableObject
{
    public PageNavItem[] Children { get; init; } = [];
    public string DisplayName { get; set; } = displayName;
    public string IconResourceKey { get; set; } = iconResourceKey;
    public string NavigateKey { get; init; } = navigateKey;
    public string? Status { get; init; }
    public bool IsNavigable { get; init; } = true;
    public bool IsSeparator { get; init; }
    public bool Selected { get; set; }
    public string? Token { get; init; } = token;

    [RelayCommand]
    private void Navigation()
    {
        if (!IsNavigable || IsSeparator)
        {
            return;
        }

        Selected = true;
        WeakReferenceMessenger.Default.Send(NavigateKey, Token ?? string.Empty);
    }
}