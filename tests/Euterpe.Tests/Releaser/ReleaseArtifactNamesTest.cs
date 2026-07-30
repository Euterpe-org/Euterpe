using Euterpe.Releaser;
using Semver;

namespace Euterpe.Tests.Releaser;

[Category("ReleaserTests")]
[TestSubject(typeof(ReleaseArtifactNames))]
public sealed class ReleaseArtifactNamesTest
{
    [Test]
    public async Task GetPackageNames_StableVersion_ReturnsVelopackNames()
    {
        var version = SemVersion.Parse("2.2.0", SemVersionStyles.Strict);

        await Assert.That(ReleaseArtifactNames.GetFullPackageFileName(version, "win-x64-stable"))
            .IsEqualTo("Euterpe-2.2.0-win-x64-stable-full.nupkg");
        await Assert.That(ReleaseArtifactNames.GetDeltaPackageFileName(version, "win-x64-stable"))
            .IsEqualTo("Euterpe-2.2.0-win-x64-stable-delta.nupkg");
    }

    [Test]
    public async Task GetInstallerFileName_SupportedRuntimes_ReturnsVelopackNames()
    {
        await Assert.That(ReleaseArtifactNames.GetInstallerFileName(
                ReleaseRuntime.Parse("win-x64"),
                "win-x64-stable"))
            .IsEqualTo("Euterpe-win-x64-stable-Setup.exe");
        await Assert.That(ReleaseArtifactNames.GetInstallerFileName(
                ReleaseRuntime.Parse("linux-x64"),
                "linux-x64-beta"))
            .IsEqualTo("Euterpe-linux-x64-beta.AppImage");
    }
}
