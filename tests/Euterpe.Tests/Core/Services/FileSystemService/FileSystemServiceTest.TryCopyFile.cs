namespace Euterpe.Tests.Core;

public sealed partial class FileSystemServiceTest
{
    [Test]
    public async Task TryCopyFile_SamePath_ReturnsTrue()
    {
        var work = NewTempFolder();
        try
        {
            var path = Path.Combine(work, "source.txt");
            await File.WriteAllTextAsync(path, "payload");

            var copied = NewService().TryCopyFile(path, path, true);

            await Assert.That(copied).IsTrue();
            await Assert.That(await File.ReadAllTextAsync(path)).IsEqualTo("payload");
        }
        finally
        {
            Directory.Delete(work, true);
        }
    }

    [Test]
    public async Task TryCopyFile_PathsDifferOnlyByCase_DefersToFileSystem()
    {
        var work = NewTempFolder();
        try
        {
            var sourcePath = Path.Combine(work, "source.txt");
            var destinationPath = Path.Combine(work, "SOURCE.txt");
            await File.WriteAllTextAsync(sourcePath, "payload");
            var destinationAlreadyExists = File.Exists(destinationPath);

            var copied = NewService().TryCopyFile(sourcePath, destinationPath, true);

            if (destinationAlreadyExists)
            {
                await Assert.That(copied).IsFalse();
                await Assert.That(await File.ReadAllTextAsync(sourcePath)).IsEqualTo("payload");
            }
            else
            {
                await Assert.That(copied).IsTrue();
                await Assert.That(File.Exists(destinationPath)).IsTrue();
                await Assert.That(await File.ReadAllTextAsync(destinationPath)).IsEqualTo("payload");
            }
        }
        finally
        {
            Directory.Delete(work, true);
        }
    }
}
