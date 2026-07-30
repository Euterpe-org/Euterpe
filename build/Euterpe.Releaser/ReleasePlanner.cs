namespace Euterpe.Releaser;

internal static class ReleasePlanner
{
    public static IReadOnlyList<string> GetBaseChannels(ReleaseRuntime runtime, SemVersion version) =>
        version.IsPrerelease
            ? [runtime.BetaChannel]
            : [runtime.StableChannel, runtime.BetaChannel];

    public static IReadOnlyList<string> GetPackageChannels(ReleaseRuntime runtime, SemVersion version, bool hasBetaBase)
    {
        if (version.IsPrerelease)
        {
            return [runtime.BetaChannel];
        }

        return hasBetaBase
            ? [runtime.StableChannel, runtime.BetaChannel]
            : [runtime.StableChannel];
    }
}
