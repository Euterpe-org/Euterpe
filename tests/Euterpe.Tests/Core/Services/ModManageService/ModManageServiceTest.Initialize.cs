using Euterpe.Contracts.Mods;

namespace Euterpe.Tests;

public sealed partial class ModManageServiceTest
{
    [Test]
    public async Task FindModByName_BeforeInit_ReturnsNull()
    {
        var sut = CreateModManageService();
        await Assert.That(sut.FindModByName(TestModName)).IsNull();
    }

    [Test]
    public async Task Connect_ReturnsObservable()
    {
        var sut = CreateModManageService();
        await Assert.That(sut.Connect()).IsNotNull();
    }

    [Test]
    public async Task InitializeModsAsync_CalledTwice_OnlyLoadsOnce()
    {
        var localServiceMock = ILocalService.Mock();
        localServiceMock.GetModFilePaths().Returns([]);
        localServiceMock.GetLibFilePaths().Returns([]);

        var sut = CreateModManageService(localService: localServiceMock);

        await sut.InitializeModsAsync();
        await sut.InitializeModsAsync();

        using var _ = Assert.Multiple();
        localServiceMock.GetModFilePaths().WasCalled(Times.Once);
        localServiceMock.GetLibFilePaths().WasCalled(Times.Once);
    }

    [Test]
    public async Task InitializeModsAsync_LoadsLocalModsIntoSourceCache()
    {
        var localServiceMock = ILocalService.Mock();
        localServiceMock.GetModFilePaths().Returns([TestModFilePath]);
        localServiceMock.GetLibFilePaths().Returns([]);
        localServiceMock.LoadModFromPathAsync(TestModFilePath).Returns(CreateInstalledMod());

        var sut = CreateModManageService(localService: localServiceMock);

        await sut.InitializeModsAsync();

        await Assert.That(sut.FindModByName(TestModName)).IsNotNull();
    }

    [Test]
    public async Task InitializeModsAsync_AddsWebOnlyModsToSourceCache()
    {
        var downloadManagerMock = IDownloadManager.Mock();
        downloadManagerMock.FetchLibListAsync(Any<CancellationToken>()).Returns([]);
        downloadManagerMock.FetchModListAsync(Any<CancellationToken>()).Returns(
        [
            new Mod
            {
                Name = "WebMod",
                Version = "2.0.0",
                FileName = "WebMod.dll",
                GameVersion = "*"
            }
        ]);

        var sut = CreateModManageService(downloadManager: downloadManagerMock);

        await sut.InitializeModsAsync();

        var webMod = sut.FindModByName("WebMod");
        using var _ = Assert.Multiple();
        await Assert.That(webMod).IsNotNull();
        await Assert.That(webMod!.IsLocal).IsFalse();
        await Assert.That(webMod.HasDownloadSource).IsTrue();
    }

    [Test]
    public async Task InitializeModsAsync_LocalVersionOlderThanWeb_MarksOutdated()
    {
        var sut = CreateModManageService(
            downloadManager: DownloadManagerWith(CreateWebMod(version: "2.0.0")),
            localService: LocalServiceWith((TestModFilePath, CreateInstalledMod())));

        await sut.InitializeModsAsync();

        await Assert.That(sut.FindModByName(TestModName)!.State).IsEqualTo(ModState.Outdated);
    }

    [Test]
    public async Task InitializeModsAsync_LocalVersionNewerThanWeb_MarksNewer()
    {
        var localMod = CreateInstalledMod();
        localMod.LocalVersion = "2.0.0";

        var sut = CreateModManageService(
            downloadManager: DownloadManagerWith(CreateWebMod(version: "1.0.0")),
            localService: LocalServiceWith((TestModFilePath, localMod)));

        await sut.InitializeModsAsync();

        await Assert.That(sut.FindModByName(TestModName)!.State).IsEqualTo(ModState.Newer);
    }

    [Test]
    public async Task InitializeModsAsync_SameVersionDifferentSha_MarksModified()
    {
        var localMod = CreateInstalledMod();
        localMod.SHA256 = "local-sha";

        var sut = CreateModManageService(
            downloadManager: DownloadManagerWith(CreateWebMod(sha256: "web-sha")),
            localService: LocalServiceWith((TestModFilePath, localMod)));

        await sut.InitializeModsAsync();

        await Assert.That(sut.FindModByName(TestModName)!.State).IsEqualTo(ModState.Modified);
    }

    [Test]
    public async Task InitializeModsAsync_SameVersionMatchingSha_MarksNormal()
    {
        var localMod = CreateInstalledMod();
        localMod.SHA256 = "shared-sha";

        var sut = CreateModManageService(
            downloadManager: DownloadManagerWith(CreateWebMod(sha256: "shared-sha")),
            localService: LocalServiceWith((TestModFilePath, localMod)));

        await sut.InitializeModsAsync();

        await Assert.That(sut.FindModByName(TestModName)!.State).IsEqualTo(ModState.Normal);
    }

    [Test]
    public async Task InitializeModsAsync_GameVersionMismatch_MarksIncompatible()
    {
        var sut = CreateModManageService(
            new Config { MuseDashFolder = TestGameFolder, GameVersion = "1.0.0" },
            DownloadManagerWith(CreateWebMod(gameVersion: "2.0.0")),
            localService: LocalServiceWith((TestModFilePath, CreateInstalledMod())));

        await sut.InitializeModsAsync();

        await Assert.That(sut.FindModByName(TestModName)!.State).IsEqualTo(ModState.Incompatible);
    }

    [Test]
    public async Task InitializeModsAsync_MelonVersionIncompatible_MarksIncompatible()
    {
        var sut = CreateModManageService(
            new Config { MuseDashFolder = TestGameFolder, MelonLoaderVersion = "0.5.0" },
            DownloadManagerWith(CreateWebMod(melonVersion: "0.6.0")),
            localService: LocalServiceWith((TestModFilePath, CreateInstalledMod())));

        await sut.InitializeModsAsync();

        await Assert.That(sut.FindModByName(TestModName)!.State).IsEqualTo(ModState.Incompatible);
    }

    [Test]
    public async Task InitializeModsAsync_DuplicatedLocalMods_MarksDuplicated()
    {
        var sut = CreateModManageService(
            localService: LocalServiceWith(
                (TestModFilePath, CreateInstalledMod()),
                ("/mods/MyMod-copy.dll", CreateInstalledMod(fileName: "MyMod-copy.dll"))));

        await sut.InitializeModsAsync();

        var mod = sut.FindModByName(TestModName);
        using var _ = Assert.Multiple();
        await Assert.That(mod!.State).IsEqualTo(ModState.Duplicated);
        await Assert.That(mod.DuplicatedModPaths).Contains(TestModFileName);
        await Assert.That(mod.DuplicatedModPaths).Contains("MyMod-copy.dll");
    }

    [Test]
    public async Task InitializeModsAsync_WebOnlyIncompatibleGameVersion_MarksIncompatible()
    {
        var sut = CreateModManageService(
            new Config { MuseDashFolder = TestGameFolder, GameVersion = "1.0.0" },
            DownloadManagerWith(CreateWebMod("WebMod", gameVersion: "2.0.0")));

        await sut.InitializeModsAsync();

        var mod = sut.FindModByName("WebMod");
        using var _ = Assert.Multiple();
        await Assert.That(mod).IsNotNull();
        await Assert.That(mod!.IsLocal).IsFalse();
        await Assert.That(mod.State).IsEqualTo(ModState.Incompatible);
    }

    [Test]
    public async Task ChangingMelonLoaderVersion_AfterInit_TransitionsCompatibleToIncompatible()
    {
        var config = new Config { MuseDashFolder = TestGameFolder, MelonLoaderVersion = "0.5.0" };
        var sut = CreateModManageService(
            config,
            DownloadManagerWith(CreateWebMod(melonVersion: "0.5.0")),
            localService: LocalServiceWith((TestModFilePath, CreateInstalledMod())));

        await sut.InitializeModsAsync();
        var mod = sut.FindModByName(TestModName);

        using var _ = Assert.Multiple();
        await Assert.That(mod!.State).IsEqualTo(ModState.Normal);

        config.MelonLoaderVersion = "0.4.0";
        await Assert.That(mod.State).IsEqualTo(ModState.Incompatible);
    }

    [Test]
    public async Task ChangingMelonLoaderVersion_AfterInit_TransitionsIncompatibleToCompatible()
    {
        var config = new Config { MuseDashFolder = TestGameFolder, MelonLoaderVersion = "0.4.0" };
        var sut = CreateModManageService(
            config,
            DownloadManagerWith(CreateWebMod(melonVersion: "0.5.0")),
            localService: LocalServiceWith((TestModFilePath, CreateInstalledMod())));

        await sut.InitializeModsAsync();
        var mod = sut.FindModByName(TestModName);

        using var _ = Assert.Multiple();
        await Assert.That(mod!.State).IsEqualTo(ModState.Incompatible);

        config.MelonLoaderVersion = "0.5.0";
        await Assert.That(mod.State).IsEqualTo(ModState.Normal);
    }

    [Test]
    public async Task ChangingMelonLoaderVersion_AfterInit_DoesNotAffectOutdatedMods()
    {
        var config = new Config { MuseDashFolder = TestGameFolder, MelonLoaderVersion = "0.5.0" };
        var sut = CreateModManageService(
            config,
            DownloadManagerWith(CreateWebMod(version: "2.0.0", melonVersion: "0.5.0")),
            localService: LocalServiceWith((TestModFilePath, CreateInstalledMod())));

        await sut.InitializeModsAsync();
        var mod = sut.FindModByName(TestModName);

        using var _ = Assert.Multiple();
        await Assert.That(mod!.State).IsEqualTo(ModState.Outdated);

        config.MelonLoaderVersion = "0.4.0";
        await Assert.That(mod.State).IsEqualTo(ModState.Outdated);
    }
}