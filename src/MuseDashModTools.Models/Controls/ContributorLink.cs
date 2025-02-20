namespace MuseDashModTools.Models.Controls;

public sealed class ContributorLink(string name, string url)
{
    public string Name { get; } = name;
    public string Url { get; } = url;

    public static implicit operator ContributorLink((string name, string url) tuple) => new(tuple.name, tuple.url);
}