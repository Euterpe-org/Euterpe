using Semver;

namespace Euterpe.Models;

public sealed class ReleaseInfo(SemVersion version)
{
    public SemVersion Version { get; init; } = version;
}