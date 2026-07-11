using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using TUnit.Core.Enums;

namespace Euterpe.Tests.Core;

[Category("WindowsGameRuntimeInstallerTests")]
[TestSubject(typeof(WindowsGameRuntimeInstaller))]
[RunOn(OS.Windows)]
[SupportedOSPlatform(nameof(OSPlatform.Windows))]
public sealed class WindowsGameRuntimeInstallerTest
{
    private string _tempDir = null!;

    [Before(Test)]
    public void Setup()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"WindowsGameRuntimeInstallerTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    [After(Test)]
    public void Cleanup()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }

    [Test]
    public async Task CheckInstalledAsync_GameLocalRuntimePresent_ReturnsTrue()
    {
        var config = new MuseDashConfig { Folder = _tempDir };
        Directory.CreateDirectory(Path.Combine(config.DotNetSharedFrameworkFolder, $"{config.DotNetRuntimeMajorVersion}.0.36"));

        var installed = await CreateInstaller(config).CheckInstalledAsync();

        await Assert.That(installed).IsTrue();
    }

    [Test]
    public async Task CheckInstalledAsync_MelonLoaderLocalRuntimePresent_ReturnsTrue()
    {
        var config = new MuseDashConfig { Folder = _tempDir };
        Directory.CreateDirectory(Path.Combine(config.MelonLoaderDotNetSharedFrameworkFolder, $"{config.DotNetRuntimeMajorVersion}.0.16"));

        var installed = await CreateInstaller(config).CheckInstalledAsync();

        await Assert.That(installed).IsTrue();
    }

    private static WindowsGameRuntimeInstaller CreateInstaller(GameConfig gameConfig) => new()
    {
        GameConfig = gameConfig,
        AppDownloadManager = IAppDownloadManager.Mock(),
        ArchiveService = IArchiveService.Mock(),
        FileSystemService = IFileSystemService.Mock(),
        Logger = Mock.Logger<WindowsGameRuntimeInstaller>()
    };
}
