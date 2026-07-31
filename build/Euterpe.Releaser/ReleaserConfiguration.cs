using static Euterpe.Shared.BuildInfo;

namespace Euterpe.Releaser;

internal static class ReleaserConfiguration
{
    public const string ApplicationProject = "src/Euterpe/Euterpe.csproj";
    public const string PackageIconPath = "src/Euterpe/Assets/Icon.ico";
    public const string PackageId = "Euterpe";

    public static SemVersion ReleaseVersion { get; } = SemVersion.Parse(AppVersion, SemVersionStyles.Strict);
}
