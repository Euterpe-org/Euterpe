namespace Euterpe.Tests;

public sealed partial class ModManageServiceTest
{
    [Test]
    public async Task ToggleModAsync_WhenDisabled_EnablesMod()
    {
        var mod = CreateInstalledMod(disabled: true);
        var fileSystemServiceMock = IFileSystemService.Mock();
        fileSystemServiceMock.TryMoveFile(Any<string>(), Any<string>()).Returns(true);
        var notificationServiceMock = INotificationService.Mock();

        var sut = CreateModManageService(
            fileSystemService: fileSystemServiceMock,
            notificationService: notificationServiceMock);

        await sut.ToggleModAsync(mod);

        using var _ = Assert.Multiple();
        await Assert.That(mod.IsDisabled).IsFalse();
        fileSystemServiceMock.TryMoveFile(Any<string>(), Any<string>()).WasCalled(Times.Once);
        notificationServiceMock.ErrorLight(Any<string>(), RefStructArg<ReadOnlySpan<object>>.Any).WasCalled(Times.Never);
    }

    [Test]
    public async Task ToggleModAsync_WhenEnabled_DisablesMod()
    {
        var mod = CreateInstalledMod(disabled: false);
        var fileSystemServiceMock = IFileSystemService.Mock();
        fileSystemServiceMock.TryMoveFile(Any<string>(), Any<string>()).Returns(true);
        var notificationServiceMock = INotificationService.Mock();

        var sut = CreateModManageService(
            fileSystemService: fileSystemServiceMock,
            notificationService: notificationServiceMock);

        await sut.ToggleModAsync(mod);

        using var _ = Assert.Multiple();
        await Assert.That(mod.IsDisabled).IsTrue();
        fileSystemServiceMock.TryMoveFile(Any<string>(), Any<string>()).WasCalled(Times.Once);
        notificationServiceMock.ErrorLight(Any<string>(), RefStructArg<ReadOnlySpan<object>>.Any).WasCalled(Times.Never);
    }

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task ToggleModAsync_WhenMoveFails_DoesNotChangeStateAndNotifiesError(bool initiallyDisabled)
    {
        var mod = CreateInstalledMod(disabled: initiallyDisabled);
        var fileSystemServiceMock = IFileSystemService.Mock();
        fileSystemServiceMock.TryMoveFile(Any<string>(), Any<string>()).Returns(false);
        var notificationServiceMock = INotificationService.Mock();

        var sut = CreateModManageService(
            fileSystemService: fileSystemServiceMock,
            notificationService: notificationServiceMock);

        await sut.ToggleModAsync(mod);

        using var _ = Assert.Multiple();
        await Assert.That(mod.IsDisabled).IsEqualTo(initiallyDisabled);
        notificationServiceMock.ErrorLight(Any<string>(), RefStructArg<ReadOnlySpan<object>>.Any).WasCalled(Times.Once);
    }
}