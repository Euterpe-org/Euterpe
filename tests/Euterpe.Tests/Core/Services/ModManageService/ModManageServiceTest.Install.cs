namespace Euterpe.Tests;

public sealed partial class ModManageServiceTest
{
    [Test]
    public async Task InstallModAsync_WhenDownloadSucceeds_AddsLocalInfoAndNotifiesSuccess()
    {
        var mod = CreateInstallableMod();
        var downloadManagerMock = IGameDownloadManager.Mock();
        downloadManagerMock.DownloadModAsync(Any<ModDto>(), Any<CancellationToken>()).Returns(true);
        downloadManagerMock.FetchModListAsync(Any<CancellationToken>()).Returns([]);
        downloadManagerMock.FetchLibListAsync(Any<CancellationToken>()).Returns([]);
        var notificationServiceMock = INotificationService.Mock();

        var sut = CreateModManageService(
            gameDownloadManager: downloadManagerMock,
            notificationService: notificationServiceMock);

        await sut.InstallModAsync(mod);

        using var _ = Assert.Multiple();
        await Assert.That(mod.IsLocal).IsTrue();
        await Assert.That(mod.LocalVersion).IsEqualTo("1.0.0");
        await Assert.That(mod.IsDisabled).IsFalse();
        notificationServiceMock.SuccessLight(Any<string>(), RefStructArg<ReadOnlySpan<object>>.Any).WasCalled(Times.Once);
        notificationServiceMock.ErrorLight(Any<string>(), RefStructArg<ReadOnlySpan<object>>.Any).WasCalled(Times.Never);
    }

    [Test]
    public async Task InstallModAsync_WhenDownloadFails_DoesNotAddLocalInfoAndNotifiesError()
    {
        var mod = CreateInstallableMod();
        var downloadManagerMock = IGameDownloadManager.Mock();
        downloadManagerMock.DownloadModAsync(Any<ModDto>(), Any<CancellationToken>()).Returns(false);
        downloadManagerMock.FetchModListAsync(Any<CancellationToken>()).Returns([]);
        downloadManagerMock.FetchLibListAsync(Any<CancellationToken>()).Returns([]);
        var notificationServiceMock = INotificationService.Mock();

        var sut = CreateModManageService(
            gameDownloadManager: downloadManagerMock,
            notificationService: notificationServiceMock);

        await sut.InstallModAsync(mod);

        using var _ = Assert.Multiple();
        await Assert.That(mod.IsLocal).IsFalse();
        await Assert.That(mod.LocalVersion).IsEmpty();
        notificationServiceMock.ErrorLight(Any<string>(), RefStructArg<ReadOnlySpan<object>>.Any).WasCalled(Times.Once);
        notificationServiceMock.SuccessLight(Any<string>(), RefStructArg<ReadOnlySpan<object>>.Any).WasCalled(Times.Never);
    }
}