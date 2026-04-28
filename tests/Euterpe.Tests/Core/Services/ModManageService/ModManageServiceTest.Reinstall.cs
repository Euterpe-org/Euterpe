namespace Euterpe.Tests;

public sealed partial class ModManageServiceTest
{
    [Test]
    public async Task ReinstallModAsync_WhenAllSucceed_ReplacesFileAndNotifiesSuccess()
    {
        var mod = CreateInstalledMod();
        var fileSystemServiceMock = IFileSystemService.Mock();
        fileSystemServiceMock.TryDeleteFile(Any<string>(), Any<DeleteOption>()).Returns(true);
        var downloadManagerMock = IDownloadManager.Mock();
        downloadManagerMock.DownloadModAsync(Any<ModDto>(), Any<CancellationToken>()).Returns(true);
        downloadManagerMock.FetchModListAsync(Any<CancellationToken>()).Returns([]);
        downloadManagerMock.FetchLibListAsync(Any<CancellationToken>()).Returns([]);
        var notificationServiceMock = INotificationService.Mock();

        var sut = CreateModManageService(
            downloadManager: downloadManagerMock,
            fileSystemService: fileSystemServiceMock,
            notificationService: notificationServiceMock);

        await sut.ReinstallModAsync(mod);

        using var _ = Assert.Multiple();
        fileSystemServiceMock.TryDeleteFile(Any<string>(), Any<DeleteOption>()).WasCalled(Times.Once);
        downloadManagerMock.DownloadModAsync(Any<ModDto>(), Any<CancellationToken>()).WasCalled(Times.Once);
        notificationServiceMock.SuccessLight(Any<string>(), RefStructArg<ReadOnlySpan<object>>.Any).WasCalled(Times.Once);
        notificationServiceMock.ErrorLight(Any<string>(), RefStructArg<ReadOnlySpan<object>>.Any).WasCalled(Times.Never);
    }

    [Test]
    public async Task ReinstallModAsync_WhenDeleteFails_DoesNotDownload()
    {
        var mod = CreateInstalledMod();
        var fileSystemServiceMock = IFileSystemService.Mock();
        fileSystemServiceMock.TryDeleteFile(Any<string>(), Any<DeleteOption>()).Returns(false);
        var downloadManagerMock = IDownloadManager.Mock();
        downloadManagerMock.FetchModListAsync(Any<CancellationToken>()).Returns([]);
        downloadManagerMock.FetchLibListAsync(Any<CancellationToken>()).Returns([]);
        var notificationServiceMock = INotificationService.Mock();

        var sut = CreateModManageService(
            downloadManager: downloadManagerMock,
            fileSystemService: fileSystemServiceMock,
            notificationService: notificationServiceMock);

        await sut.ReinstallModAsync(mod);

        using var _ = Assert.Multiple();
        downloadManagerMock.DownloadModAsync(Any<ModDto>(), Any<CancellationToken>()).WasCalled(Times.Never);
        notificationServiceMock.ErrorLight(Any<string>(), RefStructArg<ReadOnlySpan<object>>.Any).WasCalled(Times.Once);
        notificationServiceMock.SuccessLight(Any<string>(), RefStructArg<ReadOnlySpan<object>>.Any).WasCalled(Times.Never);
    }

    [Test]
    public async Task ReinstallModAsync_WhenDownloadFails_NotifiesError()
    {
        var mod = CreateInstalledMod();
        var fileSystemServiceMock = IFileSystemService.Mock();
        fileSystemServiceMock.TryDeleteFile(Any<string>(), Any<DeleteOption>()).Returns(true);
        var downloadManagerMock = IDownloadManager.Mock();
        downloadManagerMock.DownloadModAsync(Any<ModDto>(), Any<CancellationToken>()).Returns(false);
        downloadManagerMock.FetchModListAsync(Any<CancellationToken>()).Returns([]);
        downloadManagerMock.FetchLibListAsync(Any<CancellationToken>()).Returns([]);
        var notificationServiceMock = INotificationService.Mock();

        var sut = CreateModManageService(
            downloadManager: downloadManagerMock,
            fileSystemService: fileSystemServiceMock,
            notificationService: notificationServiceMock);

        await sut.ReinstallModAsync(mod);

        using var _ = Assert.Multiple();
        notificationServiceMock.ErrorLight(Any<string>(), RefStructArg<ReadOnlySpan<object>>.Any).WasCalled(Times.Once);
        notificationServiceMock.SuccessLight(Any<string>(), RefStructArg<ReadOnlySpan<object>>.Any).WasCalled(Times.Never);
    }
}