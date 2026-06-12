namespace Euterpe.Tests;

public sealed partial class ModManageServiceTest
{
    [Test]
    public async Task ReinstallModAsync_WhenAllSucceed_ReplacesFileAndNotifiesSuccess()
    {
        var mod = CreateInstalledMod();
        var fileSystemServiceMock = IFileSystemService.Mock();
        var downloadManagerMock = IGameDownloadManager.Mock();
        downloadManagerMock.FetchModListAsync(Any<CancellationToken>()).Returns([]);
        downloadManagerMock.FetchLibListAsync(Any<CancellationToken>()).Returns([]);
        var notificationServiceMock = INotificationService.Mock();

        var sut = CreateModManageService(
            gameDownloadManager: downloadManagerMock,
            fileSystemService: fileSystemServiceMock,
            notificationService: notificationServiceMock);

        await sut.ReinstallModAsync(mod);

        using var _ = Assert.Multiple();
        downloadManagerMock.DownloadModAsync(Any<ModDto>(), Any<CancellationToken>()).WasCalled(Times.Once);
        // The new file replaces the old one via an atomic overwrite move, so an enabled same-name reinstall never
        // deletes the live file first.
        fileSystemServiceMock.TryDeleteFile(Any<string>(), Any<DeleteOption>()).WasCalled(Times.Never);
        notificationServiceMock.SuccessLight(Any<string>(), RefStructArg<ReadOnlySpan<object>>.Any).WasCalled(Times.Once);
        notificationServiceMock.ErrorLight(Any<string>(), RefStructArg<ReadOnlySpan<object>>.Any).WasCalled(Times.Never);
    }

    [Test]
    public async Task ReinstallModAsync_WhenDownloadFails_NotifiesError()
    {
        var mod = CreateInstalledMod();
        var fileSystemServiceMock = IFileSystemService.Mock();
        var downloadManagerMock = IGameDownloadManager.Mock();
        downloadManagerMock.DownloadModAsync(Any<ModDto>(), Any<CancellationToken>()).Throws(new InvalidOperationException("download failed"));
        downloadManagerMock.FetchModListAsync(Any<CancellationToken>()).Returns([]);
        downloadManagerMock.FetchLibListAsync(Any<CancellationToken>()).Returns([]);
        var notificationServiceMock = INotificationService.Mock();

        var sut = CreateModManageService(
            gameDownloadManager: downloadManagerMock,
            fileSystemService: fileSystemServiceMock,
            notificationService: notificationServiceMock);

        await sut.ReinstallModAsync(mod);

        using var _ = Assert.Multiple();
        // A failed download must leave the existing file untouched: no delete is attempted.
        fileSystemServiceMock.TryDeleteFile(Any<string>(), Any<DeleteOption>()).WasCalled(Times.Never);
        notificationServiceMock.ErrorLight(Any<string>(), RefStructArg<ReadOnlySpan<object>>.Any).WasCalled(Times.Once);
        notificationServiceMock.SuccessLight(Any<string>(), RefStructArg<ReadOnlySpan<object>>.Any).WasCalled(Times.Never);
    }
}
