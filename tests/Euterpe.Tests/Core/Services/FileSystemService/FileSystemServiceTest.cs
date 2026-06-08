using Microsoft.Extensions.Logging.Abstractions;

namespace Euterpe.Tests;

[Category("FileSystemServiceTests")]
[TestSubject(typeof(FileSystemService))]
public sealed class FileSystemServiceTest
{
    private static FileSystemService NewService() => new() { Logger = NullLogger<FileSystemService>.Instance };

    private static string NewTempFolder()
    {
        var path = Path.Combine(Path.GetTempPath(), "Euterpe.Tests.Fs_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    [Test]
    public async Task DeleteFile_Existing_Deletes()
    {
        var work = NewTempFolder();
        try
        {
            var path = Path.Combine(work, "to-delete.txt");
            await File.WriteAllTextAsync(path, "x");

            NewService().DeleteFile(path);

            await Assert.That(File.Exists(path)).IsFalse();
        }
        finally
        {
            Directory.Delete(work, true);
        }
    }

    [Test]
    public async Task DeleteFile_Missing_IgnoreIfNotFound_DoesNotThrow()
    {
        var work = NewTempFolder();
        try
        {
            var act = () => NewService().DeleteFile(Path.Combine(work, "missing.txt"), DeleteOption.IgnoreIfNotFound);

            await Assert.That(act).ThrowsNothing();
        }
        finally
        {
            Directory.Delete(work, true);
        }
    }

    [Test]
    public async Task TryDeleteFile_Existing_DeletesAndReturnsTrue()
    {
        var work = NewTempFolder();
        try
        {
            var path = Path.Combine(work, "to-delete.txt");
            await File.WriteAllTextAsync(path, "x");

            var ok = NewService().TryDeleteFile(path);

            using var _ = Assert.Multiple();
            await Assert.That(ok).IsTrue();
            await Assert.That(File.Exists(path)).IsFalse();
        }
        finally
        {
            Directory.Delete(work, true);
        }
    }

    [Test]
    public async Task TryDeleteFile_Missing_IgnoreIfNotFound_ShortCircuitsToTrue()
    {
        var work = NewTempFolder();
        try
        {
            var ok = NewService().TryDeleteFile(Path.Combine(work, "missing.txt"), DeleteOption.IgnoreIfNotFound);

            await Assert.That(ok).IsTrue();
        }
        finally
        {
            Directory.Delete(work, true);
        }
    }

    [Test]
    public async Task TryMoveFile_Existing_MovesAndReturnsTrue()
    {
        var work = NewTempFolder();
        try
        {
            var src = Path.Combine(work, "src.txt");
            var dst = Path.Combine(work, "dst.txt");
            await File.WriteAllTextAsync(src, "payload");

            var ok = NewService().TryMoveFile(src, dst);

            using var _ = Assert.Multiple();
            await Assert.That(ok).IsTrue();
            await Assert.That(File.Exists(src)).IsFalse();
            await Assert.That(await File.ReadAllTextAsync(dst)).IsEqualTo("payload");
        }
        finally
        {
            Directory.Delete(work, true);
        }
    }

    [Test]
    public async Task TryMoveFile_SourceMissing_ReturnsFalse()
    {
        var work = NewTempFolder();
        try
        {
            var ok = NewService().TryMoveFile(Path.Combine(work, "missing.txt"), Path.Combine(work, "dst.txt"));

            await Assert.That(ok).IsFalse();
        }
        finally
        {
            Directory.Delete(work, true);
        }
    }

    [Test]
    public async Task DeleteDirectory_Existing_DeletesRecursively()
    {
        var work = NewTempFolder();
        try
        {
            var nested = Path.Combine(work, "nested");
            Directory.CreateDirectory(nested);
            await File.WriteAllTextAsync(Path.Combine(nested, "f.txt"), "x");

            NewService().DeleteDirectory(nested);

            await Assert.That(Directory.Exists(nested)).IsFalse();
        }
        finally
        {
            if (Directory.Exists(work))
            {
                Directory.Delete(work, true);
            }
        }
    }

    [Test]
    public async Task DeleteDirectory_Missing_FailIfNotFound_Throws()
    {
        var work = NewTempFolder();
        try
        {
            // Directory.Delete throws DirectoryNotFoundException on missing paths (unlike File.Delete which is silent).
            var act = () => NewService().DeleteDirectory(Path.Combine(work, "missing_dir"));

            await Assert.That(act).Throws<DirectoryNotFoundException>();
        }
        finally
        {
            Directory.Delete(work, true);
        }
    }

    [Test]
    public async Task DeleteDirectory_Missing_IgnoreIfNotFound_DoesNotThrow()
    {
        var work = NewTempFolder();
        try
        {
            var act = () => NewService().DeleteDirectory(Path.Combine(work, "missing_dir"), DeleteOption.IgnoreIfNotFound);

            await Assert.That(act).ThrowsNothing();
        }
        finally
        {
            Directory.Delete(work, true);
        }
    }

    [Test]
    public async Task TryDeleteDirectory_Existing_DeletesRecursively()
    {
        var work = NewTempFolder();
        try
        {
            var nested = Path.Combine(work, "nested");
            Directory.CreateDirectory(nested);
            await File.WriteAllTextAsync(Path.Combine(nested, "f.txt"), "x");

            var ok = NewService().TryDeleteDirectory(nested);

            using var _ = Assert.Multiple();
            await Assert.That(ok).IsTrue();
            await Assert.That(Directory.Exists(nested)).IsFalse();
        }
        finally
        {
            if (Directory.Exists(work))
            {
                Directory.Delete(work, true);
            }
        }
    }

    [Test]
    public async Task TryDeleteDirectory_Missing_FailIfNotFound_ReturnsFalse()
    {
        var work = NewTempFolder();
        try
        {
            // Without IgnoreIfNotFound, the catch path runs because Directory.Delete throws
            // DirectoryNotFoundException on missing paths (unlike File.Delete which is silent).
            var ok = NewService().TryDeleteDirectory(Path.Combine(work, "missing_dir"));

            await Assert.That(ok).IsFalse();
        }
        finally
        {
            Directory.Delete(work, true);
        }
    }

    [Test]
    public async Task TryDeleteDirectory_Missing_IgnoreIfNotFound_ShortCircuitsToTrue()
    {
        var work = NewTempFolder();
        try
        {
            var ok = NewService().TryDeleteDirectory(Path.Combine(work, "missing_dir"), DeleteOption.IgnoreIfNotFound);

            await Assert.That(ok).IsTrue();
        }
        finally
        {
            Directory.Delete(work, true);
        }
    }

    [Test]
    public async Task CopyDirectory_CopiesFilesAndNestedFoldersAndLeavesSource()
    {
        var work = NewTempFolder();
        try
        {
            var src = Path.Combine(work, "src");
            var nested = Path.Combine(src, "nested");
            Directory.CreateDirectory(nested);
            await File.WriteAllTextAsync(Path.Combine(src, "top.txt"), "top");
            await File.WriteAllTextAsync(Path.Combine(nested, "deep.txt"), "deep");

            var dst = Path.Combine(work, "dst");
            NewService().CopyDirectory(src, dst);

            using var _ = Assert.Multiple();
            await Assert.That(Directory.Exists(src)).IsTrue();
            await Assert.That(await File.ReadAllTextAsync(Path.Combine(dst, "top.txt"))).IsEqualTo("top");
            await Assert.That(await File.ReadAllTextAsync(Path.Combine(dst, "nested", "deep.txt"))).IsEqualTo("deep");
        }
        finally
        {
            Directory.Delete(work, true);
        }
    }

    [Test]
    public async Task CopyDirectory_SourceMissing_Throws()
    {
        var work = NewTempFolder();
        try
        {
            var act = () => NewService().CopyDirectory(Path.Combine(work, "missing"), Path.Combine(work, "dst"));

            await Assert.That(act).Throws<DirectoryNotFoundException>();
        }
        finally
        {
            Directory.Delete(work, true);
        }
    }
}