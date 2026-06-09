using Euterpe.Models.VDFs;

namespace Euterpe.Tests;

public sealed partial class GamePathServiceTest
{
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
