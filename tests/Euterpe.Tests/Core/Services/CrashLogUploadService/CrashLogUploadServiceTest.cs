using System.IO.Compression;
using System.Net;
using System.Text;
using Euterpe.Core.Http.Clients;
using Refit;
using static Euterpe.Shared.PathConstants;

namespace Euterpe.Tests.Core;

[Category("CrashLogUploadServiceTests")]
[TestSubject(typeof(CrashLogUploadService))]
public sealed class CrashLogUploadServiceTest
{
    private static CrashLogUploadService NewService(IFileSystemService fileSystem, IEuterpeLogClient? client = null) => new()
    {
        FileSystemService = fileSystem,
        LogClient = client ?? IEuterpeLogClient.Mock(),
        Logger = Mock.Logger<CrashLogUploadService>()
    };

    [Test]
    public async Task UploadAppLogAsync_LogAvailable_UploadsRawLogAsGzip()
    {
        const string rawLog = "[Critical] crash\nhttps://example.test/file?t=unredacted-token\n";
        var fileSystem = IFileSystemService.Mock();
        fileSystem.TryOpenSharedReadFile(LogFilePath).Returns(new MemoryStream(Encoding.UTF8.GetBytes(rawLog)));
        fileSystem.TryCreateTemporaryFile().Returns(new MemoryStream());
        var client = IEuterpeLogClient.Mock();
        string? uploadedLog = null;
        string? uploadedCategory = null;
        string? uploadedFileName = null;
        string? uploadedContentType = null;
        client.UploadLogAsync(Any<StreamPart>(), Any<string>(), Any<CancellationToken>())
            .Callback((file, category, _) =>
            {
                uploadedCategory = category;
                uploadedFileName = file.FileName;
                uploadedContentType = file.ContentType;
                using var gzip = new GZipStream(file.Value, CompressionMode.Decompress, true);
                using var reader = new StreamReader(gzip, Encoding.UTF8);
                uploadedLog = reader.ReadToEnd();
            })
            .Returns(new HttpResponseMessage(HttpStatusCode.NoContent));

        await NewService(fileSystem, client).UploadAppLogAsync();

        using var assertions = Assert.Multiple();
        await Assert.That(uploadedLog).IsEqualTo(rawLog);
        await Assert.That(uploadedCategory).IsEqualTo("app");
        await Assert.That(uploadedFileName).IsEqualTo($"{Path.GetFileName(LogFilePath)}.gz");
        await Assert.That(uploadedContentType).IsEqualTo("application/gzip");
        client.UploadLogAsync(Any<StreamPart>(), "app", Any<CancellationToken>()).WasCalled(Times.Once);
    }

    [Test]
    public async Task UploadAppLogAsync_LogUnavailable_DoesNotCreateTemporaryFileOrUpload()
    {
        var fileSystem = IFileSystemService.Mock();
        fileSystem.TryOpenSharedReadFile(LogFilePath).Returns((Stream?)null);
        var client = IEuterpeLogClient.Mock();

        await NewService(fileSystem, client).UploadAppLogAsync();

        fileSystem.TryCreateTemporaryFile().WasCalled(Times.Never);
        client.UploadLogAsync(Any<StreamPart>(), Any<string>(), Any<CancellationToken>()).WasCalled(Times.Never);
    }

    [Test]
    public async Task UploadAppLogAsync_ClientThrows_SwallowsException()
    {
        var fileSystem = IFileSystemService.Mock();
        fileSystem.TryOpenSharedReadFile(LogFilePath).Returns(new MemoryStream(Encoding.UTF8.GetBytes("log")));
        fileSystem.TryCreateTemporaryFile().Returns(new MemoryStream());
        var client = IEuterpeLogClient.Mock();
        client.UploadLogAsync(Any<StreamPart>(), Any<string>(), Any<CancellationToken>())
            .Throws(new HttpRequestException("server unavailable"));

        var act = async () => await NewService(fileSystem, client).UploadAppLogAsync();

        await Assert.That(act).ThrowsNothing();
    }
}
