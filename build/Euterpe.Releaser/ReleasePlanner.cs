namespace Euterpe.Releaser;

internal static class ReleasePlanner
{
    public static string GetPrimaryChannel(ReleaseRuntime runtime, SemVersion version) =>
        version.IsPrerelease ? runtime.BetaChannel : runtime.StableChannel;

    public static IReadOnlyList<string> GetBaseChannels(ReleaseRuntime runtime, SemVersion version) =>
        version.IsPrerelease
            ? [GetPrimaryChannel(runtime, version)]
            : [runtime.StableChannel, runtime.BetaChannel];

    public static IReadOnlyList<string> GetPackageChannels(ReleaseRuntime runtime, SemVersion version, bool hasBetaBase)
    {
        if (version.IsPrerelease)
        {
            return [GetPrimaryChannel(runtime, version)];
        }

        return hasBetaBase
            ? [runtime.StableChannel, runtime.BetaChannel]
            : [runtime.StableChannel];
    }
}
