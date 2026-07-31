namespace Euterpe.Releaser;

using static ReleaserConfiguration;

internal sealed record ReleaseRuntime(
    string Rid,
    string MainExecutable,
    string InstallerFileSuffix,
    IReadOnlyList<string> ExtraVpkArguments)
{
    public string StableChannel => $"{Rid}-stable";
    public string BetaChannel => $"{Rid}-beta";

    public static ReleaseRuntime Parse(string rid) =>
        rid.Split('-')[0].AsSpan() switch
        {
            "win" => new ReleaseRuntime(
                rid,
                "Euterpe.exe",
                "-Setup.exe",
                ["--noPortable", "--icon", PackageIconPath]),
            "linux" => new ReleaseRuntime(
                rid,
                "Euterpe",
                ".AppImage",
                []),
            _ => throw new ArgumentOutOfRangeException(nameof(rid), rid, "Unsupported release RID.")
        };
}
