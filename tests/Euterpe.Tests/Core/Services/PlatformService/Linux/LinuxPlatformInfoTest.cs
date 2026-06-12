using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using TUnit.Core.Enums;

namespace Euterpe.Tests.Linux;

[Category("LinuxPlatformInfoTests")]
[TestSubject(typeof(LinuxPlatformInfo))]
[RunOn(OS.Linux)]
[SupportedOSPlatform(nameof(OSPlatform.Linux))]
public sealed class LinuxPlatformInfoTest
{
    [Test]
    public async Task OsString_IsLinux()
    {
        var info = new LinuxPlatformInfo();
        await Assert.That(info.OsString).IsEqualTo("linux");
    }

    [Test]
    public async Task UpdaterFileName_IsUpdater()
    {
        var info = new LinuxPlatformInfo();
        await Assert.That(info.UpdaterFileName).IsEqualTo("Updater");
    }
}
