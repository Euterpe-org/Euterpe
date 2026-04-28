using Euterpe.Models.VDFs;
using TUnit.Mocks.Logging;

namespace Euterpe.Tests;

[Category("GamePathServiceTests")]
[TestSubject(typeof(GamePathService))]
public sealed class GamePathServiceTest
{
    private const string TestAppId = "774171";
    private const string TestRelativePath = "MuseDash";

    private readonly MockLogger<GamePathService> _logger = Mock.Logger<GamePathService>();
    private string _tempDir = null!;

    [Before(Test)]
    public void Setup()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"GamePathServiceTest_{Guid.NewGuid():N}");
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

    private GamePathService CreateService(IVdfSerializationService vdf) =>
        new()
        {
            Config = new Config { SteamFolder = _tempDir },
            Logger = _logger,
            VdfSerializationService = vdf
        };

    private void CreateVdfFileMarker()
    {
        var steamApps = Path.Combine(_tempDir, "steamapps");
        Directory.CreateDirectory(steamApps);
        File.WriteAllText(Path.Combine(steamApps, "libraryfolders.vdf"), string.Empty);
    }

    private static IVdfSerializationService CreateVdfMockReturning(Dictionary<string, LibraryFolder> libraries)
    {
        var mock = IVdfSerializationService.Mock();
        mock.DeserializeFromFile<Dictionary<string, LibraryFolder>>(Any<string>()).Returns(libraries);
        return mock;
    }

    [Test]
    public async Task TryGetGameFolderFromCommonPaths_PathExists_ReturnsTrueWithFolder()
    {
        var libA = Path.Combine(_tempDir, "LibA");
        var libB = Path.Combine(_tempDir, "LibB");
        Directory.CreateDirectory(libA);
        var expected = Path.Combine(libB, TestRelativePath);
        Directory.CreateDirectory(expected);

        var service = CreateService(IVdfSerializationService.Mock());
        var found = service.TryGetGameFolderFromCommonPaths([libA, libB], TestRelativePath, out var result);

        using var _ = Assert.Multiple();
        await Assert.That(found).IsTrue();
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    public async Task TryGetGameFolderFromCommonPaths_NoPathExists_ReturnsFalse()
    {
        var libA = Path.Combine(_tempDir, "LibA");
        Directory.CreateDirectory(libA);

        var service = CreateService(IVdfSerializationService.Mock());
        var found = service.TryGetGameFolderFromCommonPaths([libA], TestRelativePath, out var result);

        using var _ = Assert.Multiple();
        await Assert.That(found).IsFalse();
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task TryGetGameFolderFromVdf_VdfFileMissing_ReturnsFalse()
    {
        var service = CreateService(IVdfSerializationService.Mock());
        var found = service.TryGetGameFolderFromVdf(TestAppId, TestRelativePath, out var result);

        using var _ = Assert.Multiple();
        await Assert.That(found).IsFalse();
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task TryGetGameFolderFromVdf_VdfDeserializationThrows_ReturnsFalse()
    {
        CreateVdfFileMarker();
        var vdfMock = IVdfSerializationService.Mock();
        vdfMock.DeserializeFromFile<Dictionary<string, LibraryFolder>>(Any<string>())
            .Throws(new InvalidOperationException("bad vdf"));
        var service = CreateService(vdfMock);

        var found = service.TryGetGameFolderFromVdf(TestAppId, TestRelativePath, out var result);

        using var _ = Assert.Multiple();
        await Assert.That(found).IsFalse();
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task TryGetGameFolderFromVdf_AppIdInLibraryAndFolderExists_ReturnsTrue()
    {
        CreateVdfFileMarker();
        var libPath = Path.Combine(_tempDir, "Lib1");
        var expected = Path.Combine(libPath, TestRelativePath);
        Directory.CreateDirectory(expected);

        var service = CreateService(CreateVdfMockReturning(new Dictionary<string, LibraryFolder>
        {
            ["0"] = new() { Path = libPath, Apps = new Dictionary<string, string> { [TestAppId] = "1024" } }
        }));

        var found = service.TryGetGameFolderFromVdf(TestAppId, TestRelativePath, out var result);

        using var _ = Assert.Multiple();
        await Assert.That(found).IsTrue();
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    public async Task TryGetGameFolderFromVdf_AppIdNotInAnyLibrary_FallsBackToLibraryPathScan()
    {
        CreateVdfFileMarker();
        var libPath = Path.Combine(_tempDir, "Lib1");
        var expected = Path.Combine(libPath, TestRelativePath);
        Directory.CreateDirectory(expected);

        var service = CreateService(CreateVdfMockReturning(new Dictionary<string, LibraryFolder>
        {
            ["0"] = new() { Path = libPath, Apps = [] }
        }));

        var found = service.TryGetGameFolderFromVdf(TestAppId, TestRelativePath, out var result);

        using var _ = Assert.Multiple();
        await Assert.That(found).IsTrue();
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    public async Task TryGetGameFolderFromVdf_AppIdInLibraryButFolderMissing_FallsBackToScanOtherLibraries()
    {
        CreateVdfFileMarker();
        var libWithApp = Path.Combine(_tempDir, "LibWithAppEntry");
        var libWithFolder = Path.Combine(_tempDir, "LibWithGameFolder");
        Directory.CreateDirectory(libWithApp);
        var expected = Path.Combine(libWithFolder, TestRelativePath);
        Directory.CreateDirectory(expected);

        var service = CreateService(CreateVdfMockReturning(new Dictionary<string, LibraryFolder>
        {
            ["0"] = new() { Path = libWithApp, Apps = new Dictionary<string, string> { [TestAppId] = "1024" } },
            ["1"] = new() { Path = libWithFolder, Apps = [] }
        }));

        var found = service.TryGetGameFolderFromVdf(TestAppId, TestRelativePath, out var result);

        using var _ = Assert.Multiple();
        await Assert.That(found).IsTrue();
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    public async Task TryGetGameFolderFromVdf_NoLibraryHasGameFolder_ReturnsFalse()
    {
        CreateVdfFileMarker();
        var libPath = Path.Combine(_tempDir, "Lib1");
        Directory.CreateDirectory(libPath);

        var service = CreateService(CreateVdfMockReturning(new Dictionary<string, LibraryFolder>
        {
            ["0"] = new() { Path = libPath, Apps = [] }
        }));

        var found = service.TryGetGameFolderFromVdf(TestAppId, TestRelativePath, out var result);

        using var _ = Assert.Multiple();
        await Assert.That(found).IsFalse();
        await Assert.That(result).IsNull();
    }
}