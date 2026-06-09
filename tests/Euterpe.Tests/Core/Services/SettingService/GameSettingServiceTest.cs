using Microsoft.Extensions.Logging.Abstractions;

namespace Euterpe.Tests;

[Category("GameSettingServiceTests")]
[TestSubject(typeof(GameSettingService))]
public sealed class GameSettingServiceTest
{
    private static FileSystemService NewFileSystemService() => new() { Logger = NullLogger<FileSystemService>.Instance };

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task IsValidGameFolder_DelegatesToGamePathDiscovery(bool expected)
    {
        var paths = IGamePathDiscovery.Mock();
        paths.CheckIsValidGameFolder(Any<string?>()).Returns(expected);
        var service = new GameSettingService
        {
            GameConfig = new MuseDashConfig { Folder = "/some/folder" },
            GamePaths = paths,
            FileSystemService = NewFileSystemService()
        };

        await Assert.That(service.IsValidGameFolder()).IsEqualTo(expected);
    }

    [Test]
    public async Task EnsureGameFolders_CreatesAllRequiredDirectories()
    {
        var root = Path.Combine(Path.GetTempPath(), $"euterpe-test-{Guid.NewGuid():N}");
        try
        {
            var gameConfig = new MuseDashConfig { Folder = root };
            var service = new GameSettingService
            {
                GameConfig = gameConfig,
                GamePaths = IGamePathDiscovery.Mock(),
                FileSystemService = NewFileSystemService()
            };

            service.EnsureGameFolders();

            using var assertions = Assert.Multiple();
            await Assert.That(Directory.Exists(gameConfig.ModsFolder)).IsTrue();
            await Assert.That(Directory.Exists(gameConfig.UserLibsFolder)).IsTrue();
            await Assert.That(Directory.Exists(gameConfig.OnlineChartsFolder)).IsTrue();
            await Assert.That(Directory.Exists(gameConfig.OfflineChartsFolder)).IsTrue();
            await Assert.That(Directory.Exists(gameConfig.TempChartsFolder)).IsTrue();
            await Assert.That(Directory.Exists(gameConfig.TempModsFolder)).IsTrue();
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }

    [Test]
    public async Task EnsureGameFolders_WipesStaleTempContent()
    {
        var root = Path.Combine(Path.GetTempPath(), $"euterpe-test-{Guid.NewGuid():N}");
        try
        {
            var gameConfig = new MuseDashConfig { Folder = root };
            var service = new GameSettingService
            {
                GameConfig = gameConfig,
                GamePaths = IGamePathDiscovery.Mock(),
                FileSystemService = NewFileSystemService()
            };

            // Simulate an orphan left behind by a crash mid-download.
            Directory.CreateDirectory(gameConfig.TempModsFolder);
            var orphan = Path.Combine(gameConfig.TempModsFolder, "stale.dll");
            await File.WriteAllTextAsync(orphan, "partial");

            service.EnsureGameFolders();

            using var assertions = Assert.Multiple();
            await Assert.That(File.Exists(orphan)).IsFalse();
            await Assert.That(Directory.Exists(gameConfig.TempModsFolder)).IsTrue();
            await Assert.That(Directory.Exists(gameConfig.TempChartsFolder)).IsTrue();
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }
}