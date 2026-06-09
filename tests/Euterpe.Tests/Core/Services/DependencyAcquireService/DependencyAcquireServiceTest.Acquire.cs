using Downloader;

namespace Euterpe.Tests;

public sealed partial class DependencyAcquireServiceTest
{
    [Test]
    public async Task AcquireForMelonLoaderAsync_AllFilesValid_DoesNotDownload()
    {
        var sha = await CreateValidDependencyFiles();
        var client = CreateClientReturning(CreateAllMelonLoaderDeps(sha));
        var downloader = IAppDownloadManager.Mock();
        var sut = CreateService(client, downloader);

        await sut.AcquireForMelonLoaderAsync();

        downloader.DownloadFileAsync(
                Any<string>(),
                Any<string>(),
                Any<EventHandler<DownloadStartedEventArgs>?>(),
                Any<IProgress<double>?>(),
                Any<CancellationToken>())
            .WasCalled(Times.Never);
    }

    [Test]
    public async Task AcquireForMelonLoaderAsync_DownloadAlwaysFails_ThrowsAfterRetries()
    {
        var client = CreateClientReturning(CreateAllMelonLoaderDeps("expected-sha"));
        var downloader = IAppDownloadManager.Mock();
        downloader.DownloadFileAsync(
                Any<string>(),
                Any<string>(),
                Any<EventHandler<DownloadStartedEventArgs>?>(),
                Any<IProgress<double>?>(),
                Any<CancellationToken>())
            .Throws(new InvalidOperationException("download failed"));
        var sut = CreateService(client, downloader);

        var act = () => sut.AcquireForMelonLoaderAsync();

        using var _ = Assert.Multiple();
        await Assert.That(act).Throws<IOException>();
        downloader.DownloadFileAsync(
                Any<string>(),
                Any<string>(),
                Any<EventHandler<DownloadStartedEventArgs>?>(),
                Any<IProgress<double>?>(),
                Any<CancellationToken>())
            .WasCalled(Times.Exactly(3));
    }

    [Test]
    public async Task AcquireForMelonLoaderAsync_DownloadSucceedsButHashMismatch_ThrowsAfterRetries()
    {
        // Files exist with a known SHA; client expects a different SHA → all 3 attempts hit
        // the post-download hash check and fall through, eventually throwing.
        await CreateValidDependencyFiles();
        var client = CreateClientReturning(CreateAllMelonLoaderDeps("wrong-expected-sha"));
        var downloader = IAppDownloadManager.Mock();
        // Default mock returns Task.CompletedTask — equivalent to "download succeeded".
        var sut = CreateService(client, downloader);

        var act = () => sut.AcquireForMelonLoaderAsync();

        await Assert.That(act).Throws<IOException>();
    }
}