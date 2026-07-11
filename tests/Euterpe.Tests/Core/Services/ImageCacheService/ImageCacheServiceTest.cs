using System.Net;
using TUnit.Mocks.Http;
using TUnit.Mocks.Logging;

namespace Euterpe.Tests.Core;

[Category("ImageCacheServiceTests")]
[TestSubject(typeof(ImageCacheService))]
public sealed class ImageCacheServiceTest
{
    private const string ImageUrl = "https://euterpe-org.com/images/screenshot.webp";
    private static readonly Uri ImageUri = new(ImageUrl);

    private static ImageCacheService CreateService(MockHttpHandler handler, IFileSystemService fileSystem) => new()
    {
        Client = handler.CreateClient(),
        Config = new Config { CacheFolder = "Cache" },
        FileSystemService = fileSystem,
        Logger = Mock.Logger<ImageCacheService>()
    };

    [Test]
    public async Task OpenReadAsync_RelativeUri_ReturnsNull()
    {
        var service = CreateService(Mock.HttpHandler(), IFileSystemService.Mock());

        var stream = await service.OpenReadAsync(new Uri("not-a-url", UriKind.Relative));

        await Assert.That(stream).IsNull();
    }

    [Test]
    public async Task OpenReadAsync_CachedFile_ReturnsStreamWithoutDownloading()
    {
        var fileSystem = IFileSystemService.Mock();
        fileSystem.GetFileLastWriteTimeUtc(Any<string>()).Returns(DateTime.UtcNow.AddHours(-1));
        fileSystem.TryOpenReadFile(Any<string>()).Returns(new MemoryStream([1, 2, 3], false));
        var handler = Mock.HttpHandler();
        var service = CreateService(handler, fileSystem);

        await using var stream = await service.OpenReadAsync(ImageUri);

        using var _ = Assert.Multiple();
        await Assert.That(stream).IsNotNull();
        await Assert.That(stream!.ReadByte()).IsEqualTo(1);
        await Assert.That(handler.Requests.Count).IsEqualTo(0);
    }

    [Test]
    public async Task OpenReadAsync_MissingFile_DownloadsAndWritesAtomically()
    {
        var fileSystem = IFileSystemService.Mock();
        fileSystem.GetFileLastWriteTimeUtc(Any<string>()).Returns((DateTime?)null);
        fileSystem.TryWriteFileAtomicAsync(Any<string>(), Any<ReadOnlyMemory<byte>>(), Any<CancellationToken>()).Returns(true);
        fileSystem.TryOpenReadFile(Any<string>()).Returns(new MemoryStream([1, 2, 3], false));
        var handler = Mock.HttpHandler();
        handler.OnGet("/images/screenshot.webp").RespondWith(new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent([1, 2, 3]) });
        var service = CreateService(handler, fileSystem);

        await using var stream = await service.OpenReadAsync(ImageUri);

        using var _ = Assert.Multiple();
        await Assert.That(stream).IsNotNull();
        await Assert.That(stream!.ReadByte()).IsEqualTo(1);
        await Assert.That(handler.Requests.Count).IsEqualTo(1);
        fileSystem.TryWriteFileAtomicAsync(Any<string>(), Any<ReadOnlyMemory<byte>>(), Any<CancellationToken>()).WasCalled(Times.Once);
    }

    [Test]
    public async Task OpenReadAsync_CacheWriteFails_ReturnsDownloadedStream()
    {
        var fileSystem = IFileSystemService.Mock();
        fileSystem.GetFileLastWriteTimeUtc(Any<string>()).Returns((DateTime?)null);
        fileSystem.TryWriteFileAtomicAsync(Any<string>(), Any<ReadOnlyMemory<byte>>(), Any<CancellationToken>()).Returns(false);
        var handler = Mock.HttpHandler();
        handler.OnGet("/images/screenshot.webp").RespondWith(new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent([1, 2, 3]) });
        var service = CreateService(handler, fileSystem);

        await using var stream = await service.OpenReadAsync(ImageUri);

        using var _ = Assert.Multiple();
        await Assert.That(stream).IsNotNull();
        await Assert.That(stream!.ReadByte()).IsEqualTo(1);
        await Assert.That(handler.Requests.Count).IsEqualTo(1);
        fileSystem.TryOpenReadFile(Any<string>()).WasCalled(Times.Never);
    }

    [Test]
    public async Task OpenReadAsync_MissingFileAndDownloadFails_ReturnsNull()
    {
        var fileSystem = IFileSystemService.Mock();
        fileSystem.GetFileLastWriteTimeUtc(Any<string>()).Returns((DateTime?)null);
        var handler = Mock.HttpHandler();
        handler.OnGet("/images/screenshot.webp").Respond(HttpStatusCode.InternalServerError);
        var service = CreateService(handler, fileSystem);

        var stream = await service.OpenReadAsync(ImageUri);

        await Assert.That(stream).IsNull();
    }

    [Test]
    public async Task OpenReadAsync_OldCachedFile_ReturnsStreamWithoutDownloading()
    {
        var fileSystem = IFileSystemService.Mock();
        fileSystem.GetFileLastWriteTimeUtc(Any<string>()).Returns(DateTime.UtcNow.AddDays(-30));
        fileSystem.TryOpenReadFile(Any<string>()).Returns(new MemoryStream([1, 2, 3], false));
        var handler = Mock.HttpHandler();
        var service = CreateService(handler, fileSystem);

        await using var stream = await service.OpenReadAsync(ImageUri);

        using var _ = Assert.Multiple();
        await Assert.That(stream).IsNotNull();
        await Assert.That(handler.Requests.Count).IsEqualTo(0);
    }
}
