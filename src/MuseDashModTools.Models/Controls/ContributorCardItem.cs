namespace MuseDashModTools.Models.Controls;

public class ContributorCardItem
{
    public readonly record struct Link(string Name, string Url);

    public string Name { get; init; }
    public string? Description { get; init; }
    public string? AvatarPath { get; init; }
    public List<Link>? Links { get; init; }

    public ContributorCardItem(string name, string? description = null, string? avatarPath = null, List<(string, string)>? links = null)
    {
        Name = name;
        Description = description;
        AvatarPath = avatarPath ?? $"avares://MuseDashModTools/Assets/Contributors/{name.Replace(' ', '_')}.webp";
        Links = links?.Select(x => new Link(x.Item1, x.Item2)).ToList();
    }
}