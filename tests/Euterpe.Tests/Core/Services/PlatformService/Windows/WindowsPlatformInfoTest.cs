using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using TUnit.Core.Enums;

namespace Euterpe.Tests.Windows;

[Category("WindowsPlatformInfoTests")]
[TestSubject(typeof(WindowsPlatformInfo))]
[RunOn(OS.Windows)]
[SupportedOSPlatform(nameof(OSPlatform.Windows))]
public sealed class WindowsPlatformInfoTest
{
    [Test]
    public async Task OsString_IsWin()
    {
        var info = new WindowsPlatformInfo();
        await Assert.That(info.OsString).IsEqualTo("win");
    }

    [Test]
    public async Task UpdaterFileName_IsUpdaterExe()
    {
        var info = new WindowsPlatformInfo();
        await Assert.That(info.UpdaterFileName).IsEqualTo("Updater.exe");
    }
}