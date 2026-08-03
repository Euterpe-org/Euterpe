namespace Euterpe.Tests.Core;

public sealed partial class FileSystemServiceTest
{
    [Test]
    public async Task TryOpenSharedReadFile_OpenLog_AllowsConcurrentAppend()
    {
        var work = NewTempFolder();
        try
        {
            var path = Path.Combine(work, "app.log");
            await File.WriteAllTextAsync(path, "before");

            await using var stream = NewService().TryOpenSharedReadFile(path);
            await File.AppendAllTextAsync(path, " after");
            using var reader = new StreamReader(stream!);

            using var assertions = Assert.Multiple();
            await Assert.That(stream).IsNotNull();
            await Assert.That(await reader.ReadToEndAsync()).IsEqualTo("before after");
        }
        finally
        {
            Directory.Delete(work, true);
        }
    }

    [Test]
    public async Task TryCreateTemporaryFile_Disposed_DeletesFile()
    {
        var stream = NewService().TryCreateTemporaryFile();
        var fileStream = stream as FileStream;
        var path = fileStream?.Name;

        await stream!.WriteAsync("payload"u8.ToArray());
        await stream.DisposeAsync();

        using var assertions = Assert.Multiple();
        await Assert.That(fileStream).IsNotNull();
        await Assert.That(path).IsNotNull();
        await Assert.That(File.Exists(path)).IsFalse();
    }
}
