using System.Collections.Immutable;

namespace MuseDashModTools.Models;

public static class GitHubResources
{
    public static readonly Dictionary<string, string> ReadmeCache = [];

    public static ImmutableArray<string> CommonReadmeNames { get; } = ["README.md", "readme.md", "Readme.md", "ReadMe.md", "READMe.MD"];

    public static ImmutableArray<string> Branches { get; } = ["main", "master"];
}