namespace Euterpe.Styles.Models;

public sealed record ContributorLink(string Name, string Url)
{
    public static implicit operator ContributorLink((string name, string url) tuple) => new(tuple.name, tuple.url);
}