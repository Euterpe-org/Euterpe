namespace Euterpe.Tests.Core;

public sealed partial class GamePathServiceTest
{
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
}
