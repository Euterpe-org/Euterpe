namespace MuseDashModTools.Models.Controls;

public sealed class NavItem(string displayName, string navigateKey, string iconResourceKey = "") : ObservableObject
{
    public NavItem[] Children { get; init; } = [];
    public string DisplayName { get; set; } = displayName;
    public string NavigateKey { get; init; } = navigateKey;
    public string IconResourceKey { get; set; } = iconResourceKey;
    public string? Status { get; init; }
    public bool IsNavigable { get; init; } = true;
    public bool IsSeparator { get; init; }
    public bool ShowAtBottom { get; init; }
}