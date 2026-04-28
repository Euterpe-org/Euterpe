namespace Euterpe.Tests;

public sealed partial class ModManageServiceTest
{
    [Test]
    public async Task UninstallModAsync_WhenDeleteSucceeds_RemovesLocalInfoAndNotifiesSuccess()
    {
        var mod = CreateInstalledMod();
        var fileSystemServiceMock = IFileSystemService.Mock();
        fileSystemServiceMock.TryDeleteFile(Any<string>(), Any<DeleteOption>()).Returns(true);
        var notificationServiceMock = INotificationService.Mock();

        var sut = CreateModManageService(
            fileSystemService: fileSystemServiceMock,
            notificationService: notificationServiceMock);

        await sut.UninstallModAsync(mod);

        using var _ = Assert.Multiple();
        await Assert.That(mod.IsLocal).IsFalse();
        await Assert.That(mod.LocalVersion).IsEmpty();
        await Assert.That(mod.IsDisabled).IsTrue();
        fileSystemServiceMock.TryDeleteFile(Any<string>(), Any<DeleteOption>()).WasCalled(Times.Once);
        notificationServiceMock.SuccessLight(Any<string>(), RefStructArg<ReadOnlySpan<object>>.Any).WasCalled(Times.Once);
        notificationServiceMock.ErrorLight(Any<string>(), RefStructArg<ReadOnlySpan<object>>.Any).WasCalled(Times.Never);
    }

    [Test]
    public async Task UninstallModAsync_WhenDeleteFails_KeepsLocalInfoAndNotifiesError()
    {
        var mod = CreateInstalledMod();
        var fileSystemServiceMock = IFileSystemService.Mock();
        fileSystemServiceMock.TryDeleteFile(Any<string>(), Any<DeleteOption>()).Returns(false);
        var notificationServiceMock = INotificationService.Mock();

        var sut = CreateModManageService(
            fileSystemService: fileSystemServiceMock,
            notificationService: notificationServiceMock);

        await sut.UninstallModAsync(mod);

        using var _ = Assert.Multiple();
        await Assert.That(mod.IsLocal).IsTrue();
        await Assert.That(mod.LocalVersion).IsEqualTo("1.0.0");
        notificationServiceMock.ErrorLight(Any<string>(), RefStructArg<ReadOnlySpan<object>>.Any).WasCalled(Times.Once);
        notificationServiceMock.SuccessLight(Any<string>(), RefStructArg<ReadOnlySpan<object>>.Any).WasCalled(Times.Never);
    }
}