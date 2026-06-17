using System.Collections.Immutable;

namespace Euterpe.Core;

internal static class GitHubResources
{
    internal static readonly Dictionary<string, string> ReadmeCache = [];
    internal static ImmutableArray<string> CommonReadmeNames { get; } = ["README.md", "readme.md", "Readme.md", "ReadMe.md", "README.MD"];
    internal static ImmutableArray<string> Branches { get; } = ["main", "master"];
}
