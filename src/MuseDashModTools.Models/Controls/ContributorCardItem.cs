using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace MuseDashModTools.Models.Controls;

public sealed class ContributorCardItem
{
    public string Name { get; }
    public Bitmap Avatar { get; }
    public string? Description { get; }
    public ContributorLink[]? Links { get; }

    public ContributorCardItem(string name, string? description = null, ContributorLink[]? links = null, string? avatarName = null)
    {
        Name = name;
        Description = description;
        Links = links;

        var avatarPath = avatarName is null ? $"{name}.webp" : $"{avatarName}.webp";
        Avatar = new Bitmap(AssetLoader.Open(new Uri($"avares://{nameof(MuseDashModTools)}/Assets/Contributors/{avatarPath}")));
    }
}