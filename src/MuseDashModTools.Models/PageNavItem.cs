using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace MuseDashModTools.Models;

public sealed partial class PageNavItem(string displayName, string iconResourceKey, string navigateKey, string token) : ObservableObject
{
    [ObservableProperty]
    public partial bool Selected { get; set; }

    public PageNavItem[] Children { get; init; } = [];
    public string DisplayName { get; set; } = displayName;
    public string IconResourceKey { get; set; } = iconResourceKey;
    public string NavigateKey { get; init; } = navigateKey;
    public string Token { get; init; } = token;
    public string? Status { get; init; }
    public bool IsNavigable { get; init; } = true;
    public bool IsSeparator { get; init; }

    [RelayCommand]
    private void Navigate()
    {
        if (!IsNavigable || IsSeparator)
        {
            return;
        }

        Selected = true;

        WeakReferenceMessenger.Default.Send(NavigateKey, Token);
    }
}