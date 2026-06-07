using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Euterpe.Shared;

namespace Euterpe.Controls.Models;

public sealed class Contributor
{
    public string Name { get; }
    public Bitmap Avatar { get; }
    public string? Description { get; }
    public ContributorLink[]? Links { get; }

    public Contributor(string name, string? description = null, ContributorLink[]? links = null, string? avatarName = null)
    {
        Name = name;
        Description = description;
        Links = links;

        var avatarPath = avatarName is null ? $"{name}.webp" : $"{avatarName}.webp";
        Avatar = new Bitmap(AssetLoader.Open(AppAssets.Uri($"Contributors/{avatarPath}")));
    }
}