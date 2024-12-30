using System.Collections.Immutable;

namespace MuseDashModTools.Common;

public static class GitHubResources
{
    public static readonly Dictionary<string, string> ReadmeCache = [];

    public static ImmutableArray<string> CommonReadmeNames { get; } = ["README.md", "readme.md", "Readme.md", "ReadMe.md", "README.MD"];

    public static ImmutableArray<string> Branches { get; } = ["main", "master"];
}