using Euterpe.Releaser;

using static Euterpe.Releaser.ReleaserConfiguration;

namespace Euterpe.Tests.Releaser;

[Category("ReleaserTests")]
[TestSubject(typeof(ReleaseRuntime))]
public sealed class ReleaseRuntimeTest
{
    [Test]
    [Arguments("win-x64", "Euterpe.exe", "-Setup.exe", true)]
    [Arguments("win-arm64", "Euterpe.exe", "-Setup.exe", true)]
    [Arguments("linux-x64", "Euterpe", ".AppImage", false)]
    [Arguments("linux-arm64", "Euterpe", ".AppImage", false)]
    public async Task Parse_SupportedRid_ReturnsRuntime(
        string rid,
        string expectedExecutable,
        string expectedInstallerFileSuffix,
        bool expectedDisablePortable)
    {
        var runtime = ReleaseRuntime.Parse(rid);

        await Assert.That(runtime.MainExecutable).IsEqualTo(expectedExecutable);
        await Assert.That(runtime.InstallerFileSuffix).IsEqualTo(expectedInstallerFileSuffix);
        await Assert.That(runtime.ExtraVpkArguments.SequenceEqual(
                expectedDisablePortable
                    ? ["--noPortable", "--icon", PackageIconPath]
                    : []))
            .IsTrue();
        await Assert.That(runtime.StableChannel).IsEqualTo($"{rid}-stable");
        await Assert.That(runtime.BetaChannel).IsEqualTo($"{rid}-beta");
    }
}
