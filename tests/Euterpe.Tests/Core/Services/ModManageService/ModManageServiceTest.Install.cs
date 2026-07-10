namespace Euterpe.Tests.Core;

public sealed partial class ModManageServiceTest
{
    [Test]
    public async Task InstallModAsync_WhenDownloadSucceeds_AddsLocalInfoAndNotifiesSuccess()
    {
        var mod = CreateInstallableMod();
        var downloadManagerMock = IGameDownloadManager.Mock();
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
        downloadManagerMock.DownloadModAsync(Any<ModDto>(), Any<CancellationToken>()).Throws(new InvalidOperationException("download failed"));
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

    [Test]
    public async Task InstallModAsync_WhenConflictsWithAnotherWebMod_MarksTheOtherWebModIncompatible()
    {
        var sut = CreateModManageService(
            gameDownloadManager: DownloadManagerWith(
                CreateWebMod("ModA", incompatibleMods: ["ModB"]),
                CreateWebMod("ModB")));
        await sut.InitializeModsAsync();

        var modA = sut.FindModByName("ModA")!;
        var modB = sut.FindModByName("ModB")!;
        await Assert.That(modB.IsInstallable).IsTrue();

        await sut.InstallModAsync(modA);

        using var _ = Assert.Multiple();
        await Assert.That(modA.State).IsEqualTo(ModState.Normal);
        await Assert.That(modB.State).IsEqualTo(ModState.Incompatible);
        await Assert.That(modB.IsInstallable).IsFalse();
    }

    [Test]
    public async Task InstallModsAsync_UnknownMod_ReportsSkipped()
    {
        var sut = CreateModManageService();
        await sut.InitializeModsAsync();

        var results = await sut.InstallModsAsync([new ModInstallRequest("Ghost", false)]);

        await Assert.That(results.Single().Outcome).IsEqualTo(BulkItemOutcome.Skipped);
    }

    [Test]
    public async Task InstallModsAsync_InstallableMod_InstallsWithoutPerItemToast()
    {
        var notificationServiceMock = INotificationService.Mock();
        var sut = CreateModManageService(
            gameDownloadManager: DownloadManagerWith(CreateWebMod("ModA")),
            notificationService: notificationServiceMock);
        await sut.InitializeModsAsync();

        var results = await sut.InstallModsAsync([new ModInstallRequest("ModA", false)]);

        using var _ = Assert.Multiple();
        await Assert.That(results.Single().Outcome).IsEqualTo(BulkItemOutcome.Added);
        await Assert.That(sut.FindModByName("ModA")!.IsLocal).IsTrue();
        notificationServiceMock.SuccessLight(Any<string>(), RefStructArg<ReadOnlySpan<object>>.Any).WasCalled(Times.Never);
    }

    [Test]
    public async Task InstallModsAsync_AlreadyInstalledMod_ReportsAlreadyPresentWithoutReinstalling()
    {
        var downloadManagerMock = IGameDownloadManager.Mock();
        downloadManagerMock.FetchModListAsync(Any<CancellationToken>()).Returns([CreateWebMod("ModA")]);
        downloadManagerMock.FetchLibListAsync(Any<CancellationToken>()).Returns([]);
        var sut = CreateModManageService(
            gameDownloadManager: downloadManagerMock,
            modLocalService: LocalServiceWith((TestModFilePath, CreateInstalledMod("ModA", "ModA.dll"))));
        await sut.InitializeModsAsync();

        var results = await sut.InstallModsAsync([new ModInstallRequest("ModA", false)]);

        using var _ = Assert.Multiple();
        await Assert.That(results.Single().Outcome).IsEqualTo(BulkItemOutcome.AlreadyPresent);
        downloadManagerMock.DownloadModAsync(Any<ModDto>(), Any<CancellationToken>()).WasCalled(Times.Never);
    }

    [Test]
    public async Task InstallModsAsync_RequestedDisabled_InstallsThenDisablesMod()
    {
        var fileSystemMock = IFileSystemService.Mock();
        fileSystemMock.TryMoveFile(Any<string>(), Any<string>(), Any<bool>()).Returns(true);
        var sut = CreateModManageService(
            gameDownloadManager: DownloadManagerWith(CreateWebMod("ModA")),
            fileSystemService: fileSystemMock);
        await sut.InitializeModsAsync();

        await sut.InstallModsAsync([new ModInstallRequest("ModA", true)]);

        await Assert.That(sut.FindModByName("ModA")!.IsDisabled).IsTrue();
    }

    [Test]
    public async Task InstallModsAsync_InstalledMod_AppliesRequestedDisabledState()
    {
        var fileSystemMock = IFileSystemService.Mock();
        fileSystemMock.TryMoveFile(Any<string>(), Any<string>(), Any<bool>()).Returns(true);
        var sut = CreateModManageService(
            gameDownloadManager: DownloadManagerWith(CreateWebMod("ModA")),
            fileSystemService: fileSystemMock,
            modLocalService: LocalServiceWith((TestModFilePath, CreateInstalledMod("ModA", "ModA.dll"))));
        await sut.InitializeModsAsync();

        var results = await sut.InstallModsAsync([new ModInstallRequest("ModA", true)]);

        using var _ = Assert.Multiple();
        await Assert.That(results.Single().Outcome).IsEqualTo(BulkItemOutcome.AlreadyPresent);
        await Assert.That(sut.FindModByName("ModA")!.IsDisabled).IsTrue();
    }
}
