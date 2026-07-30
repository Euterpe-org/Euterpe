namespace Euterpe.Releaser;

internal static class ReleaseArtifactNames
{
    public static string GetFullPackageFileName(SemVersion version, string channel) =>
        $"{PackageId}-{version}-{channel}-full.nupkg";

    public static string GetDeltaPackageFileName(SemVersion version, string channel) =>
        $"{PackageId}-{version}-{channel}-delta.nupkg";

    public static string GetInstallerFileName(ReleaseRuntime runtime, string channel) =>
        $"{PackageId}-{channel}{runtime.InstallerFileSuffix}";
}
