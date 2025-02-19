using System.Collections.ObjectModel;
using Avalonia.Media.Imaging;

namespace MuseDashModTools.Models.Controls;

public sealed class ContributorCardItem(string name, string? description = null, Bitmap? avatar = null, ObservableCollection<ContributorCardLinkItem>? links = null)
{
    public string Name { get; } = name;
    public string? Description { get; init; } = description;
    public Bitmap? Avatar { get; init; } = avatar;
    public ObservableCollection<ContributorCardLinkItem>? Links { get; init; } = links;
}