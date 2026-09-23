using Cake.Core.IO;

namespace Euterpe.Releaser;

public sealed partial class ReleaseContext
{
    public DirectoryPath RepositoryRoot => Environment.WorkingDirectory;
    public DirectoryPath ApplicationDirectory => RepositoryRoot.Combine($"artifacts/publish/Euterpe/release_{Rid}");
    public DirectoryPath GitHubReleaseDirectory => RepositoryRoot.Combine("artifacts/github-release");
    public string GitHubInstallerFileName => $"{PackageId}-{Rid}{InstallerFileSuffix}";

    public DirectoryPath GetPackageDirectory(string channel) => RepositoryRoot.Combine($"artifacts/releases/{channel}");

    public string GetFullPackageFileName(string channel, string? version = null) =>
        $"{PackageId}-{version ?? Version.ToString()}-{channel}-full.nupkg";

    public string GetDeltaPackageFileName(string channel) => $"{PackageId}-{Version}-{channel}-delta.nupkg";

    public string GetInstallerFileName(string channel) => $"{PackageId}-{channel}{InstallerFileSuffix}";
}
