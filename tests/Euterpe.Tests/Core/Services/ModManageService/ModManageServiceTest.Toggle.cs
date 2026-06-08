namespace Euterpe.Tests;

public sealed partial class ModManageServiceTest
{
    [Test]
    public async Task ToggleModAsync_WhenDisabled_EnablesMod()
    {
        var mod = CreateInstalledMod(disabled: true);
        var fileSystemServiceMock = IFileSystemService.Mock();
        fileSystemServiceMock.TryMoveFile(Any<string>(), Any<string>(), Any<bool>()).Returns(true);
        var notificationServiceMock = INotificationService.Mock();

        var sut = CreateModManageService(
            fileSystemService: fileSystemServiceMock,
            notificationService: notificationServiceMock);

        await sut.ToggleModAsync(mod);

        using var _ = Assert.Multiple();
        await Assert.That(mod.IsDisabled).IsFalse();
        fileSystemServiceMock.TryMoveFile(Any<string>(), Any<string>(), Any<bool>()).WasCalled(Times.Once);
        notificationServiceMock.ErrorLight(Any<string>(), RefStructArg<ReadOnlySpan<object>>.Any).WasCalled(Times.Never);
    }

    [Test]
    public async Task ToggleModAsync_WhenEnabled_DisablesMod()
    {
        var mod = CreateInstalledMod(disabled: false);
        var fileSystemServiceMock = IFileSystemService.Mock();
        fileSystemServiceMock.TryMoveFile(Any<string>(), Any<string>(), Any<bool>()).Returns(true);
        var notificationServiceMock = INotificationService.Mock();

        var sut = CreateModManageService(
            fileSystemService: fileSystemServiceMock,
            notificationService: notificationServiceMock);

        await sut.ToggleModAsync(mod);

        using var _ = Assert.Multiple();
        await Assert.That(mod.IsDisabled).IsTrue();
        fileSystemServiceMock.TryMoveFile(Any<string>(), Any<string>(), Any<bool>()).WasCalled(Times.Once);
        notificationServiceMock.ErrorLight(Any<string>(), RefStructArg<ReadOnlySpan<object>>.Any).WasCalled(Times.Never);
    }

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task ToggleModAsync_WhenMoveFails_DoesNotChangeStateAndNotifiesError(bool initiallyDisabled)
    {
        var mod = CreateInstalledMod(disabled: initiallyDisabled);
        var fileSystemServiceMock = IFileSystemService.Mock();
        fileSystemServiceMock.TryMoveFile(Any<string>(), Any<string>(), Any<bool>()).Returns(false);
        var notificationServiceMock = INotificationService.Mock();

        var sut = CreateModManageService(
            fileSystemService: fileSystemServiceMock,
            notificationService: notificationServiceMock);

        await sut.ToggleModAsync(mod);

        using var _ = Assert.Multiple();
        await Assert.That(mod.IsDisabled).IsEqualTo(initiallyDisabled);
        notificationServiceMock.ErrorLight(Any<string>(), RefStructArg<ReadOnlySpan<object>>.Any).WasCalled(Times.Once);
    }

    [Test]
    public async Task ToggleModAsync_EnableMod_AlsoEnablesDisabledLocalDependency()
    {
        var dep = CreateInstalledMod("ModB", "ModB.dll", true);
        var mod = CreateInstalledMod("ModA", "ModA.dll", true);
        mod.ModDependencies = ["ModB"];

        var fileSystemServiceMock = IFileSystemService.Mock();
        fileSystemServiceMock.TryMoveFile(Any<string>(), Any<string>(), Any<bool>()).Returns(true);

        var sut = CreateModManageService(
            fileSystemService: fileSystemServiceMock,
            modLocalService: LocalServiceWith(
                ("/mods/ModA.disabled", mod),
                ("/mods/ModB.disabled", dep)));

        await sut.InitializeModsAsync();
        await sut.ToggleModAsync(mod);

        using var _ = Assert.Multiple();
        await Assert.That(mod.IsDisabled).IsFalse();
        await Assert.That(dep.IsDisabled).IsFalse();
        fileSystemServiceMock.TryMoveFile(Any<string>(), Any<string>(), Any<bool>()).WasCalled(Times.Exactly(2));
    }

    [Test]
    public async Task ToggleModAsync_DisableMod_AlsoDisablesEnabledLocalDependent()
    {
        var dep = CreateInstalledMod("ModB", "ModB.dll");
        var mod = CreateInstalledMod("ModA", "ModA.dll");
        mod.ModDependencies = ["ModB"];

        var fileSystemServiceMock = IFileSystemService.Mock();
        fileSystemServiceMock.TryMoveFile(Any<string>(), Any<string>(), Any<bool>()).Returns(true);

        var sut = CreateModManageService(
            fileSystemService: fileSystemServiceMock,
            modLocalService: LocalServiceWith(
                ("/mods/ModA.dll", mod),
                ("/mods/ModB.dll", dep)));

        await sut.InitializeModsAsync();
        await sut.ToggleModAsync(dep);

        using var _ = Assert.Multiple();
        await Assert.That(dep.IsDisabled).IsTrue();
        await Assert.That(mod.IsDisabled).IsTrue();
        fileSystemServiceMock.TryMoveFile(Any<string>(), Any<string>(), Any<bool>()).WasCalled(Times.Exactly(2));
    }
}