namespace Euterpe.Tests;

public sealed partial class ModManageServiceTest
{
    [Test]
    public async Task InitializeModsAsync_LocalLibSameShaAsWeb_DoesNotTriggerDownload()
    {
        var localLib = new LibDto { Name = TestLibName, FileName = TestLibFileName, SHA256 = "shared-sha", IsLocal = true };
        var localServiceMock = IGameLocalService.Mock();
        localServiceMock.GetModFilePaths().Returns([]);
        localServiceMock.GetLibFilePaths().Returns([TestLibFilePath]);
        localServiceMock.LoadLibFromPathAsync(TestLibFilePath).Returns(localLib);

        var downloadManagerMock = IDownloadManager.Mock();
        downloadManagerMock.FetchModListAsync(Any<CancellationToken>()).Returns([]);
        downloadManagerMock.FetchLibListAsync(Any<CancellationToken>()).Returns([CreateWebLib(TestLibName, "shared-sha")]);

        var sut = CreateModManageService(
            downloadManager: downloadManagerMock,
            gameLocalService: localServiceMock);

        await sut.InitializeModsAsync();

        downloadManagerMock.DownloadLibAsync(Any<LibDto>(), Any<CancellationToken>()).WasCalled(Times.Never);
    }

    [Test]
    public async Task InitializeModsAsync_LocalLibDifferentShaFromWeb_TriggersDownload()
    {
        var localLib = new LibDto { Name = TestLibName, FileName = TestLibFileName, SHA256 = "old-sha", IsLocal = true };
        var localServiceMock = IGameLocalService.Mock();
        localServiceMock.GetModFilePaths().Returns([]);
        localServiceMock.GetLibFilePaths().Returns([TestLibFilePath]);
        localServiceMock.LoadLibFromPathAsync(Any<string>()).Returns(localLib);

        var downloadManagerMock = IDownloadManager.Mock();
        downloadManagerMock.FetchModListAsync(Any<CancellationToken>()).Returns([]);
        downloadManagerMock.FetchLibListAsync(Any<CancellationToken>()).Returns([CreateWebLib(TestLibName, "new-sha")]);
        downloadManagerMock.DownloadLibAsync(Any<LibDto>(), Any<CancellationToken>()).Returns(true);

        var sut = CreateModManageService(
            downloadManager: downloadManagerMock,
            gameLocalService: localServiceMock);

        await sut.InitializeModsAsync();

        downloadManagerMock.DownloadLibAsync(Any<LibDto>(), Any<CancellationToken>()).WasCalled(Times.AtLeastOnce);
    }

    [Test]
    public async Task InitializeModsAsync_ModDependsOnWebOnlyLib_TriggersDownload()
    {
        var localServiceMock = IGameLocalService.Mock();
        localServiceMock.GetModFilePaths().Returns([TestModFilePath]);
        localServiceMock.LoadModFromPathAsync(TestModFilePath).Returns(CreateInstalledMod());
        localServiceMock.GetLibFilePaths().Returns([]);
        localServiceMock.LoadLibFromPathAsync(Any<string>()).Returns(new LibDto { Name = TestLibName, FileName = TestLibFileName, SHA256 = "lib-sha", IsLocal = true });

        var downloadManagerMock = IDownloadManager.Mock();
        downloadManagerMock.FetchModListAsync(Any<CancellationToken>()).Returns([CreateWebMod(libDependencies: [TestLibName])]);
        downloadManagerMock.FetchLibListAsync(Any<CancellationToken>()).Returns([CreateWebLib(TestLibName, "lib-sha")]);
        downloadManagerMock.DownloadLibAsync(Any<LibDto>(), Any<CancellationToken>()).Returns(true);

        var sut = CreateModManageService(
            downloadManager: downloadManagerMock,
            gameLocalService: localServiceMock);

        await sut.InitializeModsAsync();

        downloadManagerMock.DownloadLibAsync(Any<LibDto>(), Any<CancellationToken>()).WasCalled(Times.AtLeastOnce);
    }
}