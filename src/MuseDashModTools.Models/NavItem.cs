namespace MuseDashModTools.Models;

public sealed partial class NavItem(string displayName, string iconResourceKey, string navigateKey) : ObservableObject
{
    [ObservableProperty]
    public partial bool Selected { get; set; }

    public NavItem[] Children { get; init; } = [];
    public string DisplayName { get; set; } = displayName;
    public string IconResourceKey { get; set; } = iconResourceKey;
    public string NavigateKey { get; init; } = navigateKey;
    public string? Status { get; init; }
    public bool IsNavigable { get; init; } = true;
    public bool IsSeparator { get; init; }
    public bool ShowAtBottom { get; init; }
}