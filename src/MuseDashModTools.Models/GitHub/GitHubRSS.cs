using Semver;

namespace MuseDashModTools.Models.GitHub;

public sealed class GitHubRSS(SemVersion version)
{
    public SemVersion Version { get; init; } = version;
}