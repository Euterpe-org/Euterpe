namespace MuseDashModTools.Models.Controls;

public sealed class ContributorCardLinkItem(string displayName, string url, string? icon = null)
{
    public string DisplayName { get; init; } = displayName;
    public string? Icon { get; init; } = icon;
    public string Url { get; init; } = url;
}