namespace Euterpe.Tests.Core;

public sealed partial class ModManageServiceTest
{
    [Test]
    public async Task InitializeModsAsync_LocalVersionOlderThanWeb_MarksOutdated()
    {
        var sut = CreateModManageService(
            gameDownloadManager: DownloadManagerWith(CreateWebMod(version: "2.0.0")),
            modLocalService: LocalServiceWith((TestModFilePath, CreateInstalledMod())));

        await sut.InitializeModsAsync();

        await Assert.That(sut.FindModByName(TestModName)!.State).IsEqualTo(ModState.Outdated);
    }

    [Test]
    public async Task InitializeModsAsync_LocalVersionNewerThanWeb_MarksNewer()
    {
        var localMod = CreateInstalledMod();
        localMod.LocalVersion = "2.0.0";

        var sut = CreateModManageService(
            gameDownloadManager: DownloadManagerWith(CreateWebMod(version: "1.0.0")),
            modLocalService: LocalServiceWith((TestModFilePath, localMod)));

        await sut.InitializeModsAsync();

        await Assert.That(sut.FindModByName(TestModName)!.State).IsEqualTo(ModState.Newer);
    }

    [Test]
    public async Task InitializeModsAsync_SameVersionDifferentSha_MarksModified()
    {
        var localMod = CreateInstalledMod();
        localMod.LocalSHA256 = "local-sha";

        var sut = CreateModManageService(
            gameDownloadManager: DownloadManagerWith(CreateWebMod(sha256: "web-sha")),
            modLocalService: LocalServiceWith((TestModFilePath, localMod)));

        await sut.InitializeModsAsync();

        await Assert.That(sut.FindModByName(TestModName)!.State).IsEqualTo(ModState.Modified);
    }

    [Test]
    public async Task InitializeModsAsync_SameVersionMatchingSha_MarksNormal()
    {
        var localMod = CreateInstalledMod();
        localMod.LocalSHA256 = "shared-sha";

        var sut = CreateModManageService(
            gameDownloadManager: DownloadManagerWith(CreateWebMod(sha256: "shared-sha")),
            modLocalService: LocalServiceWith((TestModFilePath, localMod)));

        await sut.InitializeModsAsync();

        await Assert.That(sut.FindModByName(TestModName)!.State).IsEqualTo(ModState.Normal);
    }

    [Test]
    public async Task InitializeModsAsync_GameVersionMismatch_MarksIncompatible()
    {
        var sut = CreateModManageService(
            CreateGame("1.0.0"),
            DownloadManagerWith(CreateWebMod(gameVersion: "2.0.0")),
            modLocalService: LocalServiceWith((TestModFilePath, CreateInstalledMod())));

        await sut.InitializeModsAsync();

        var mod = sut.FindModByName(TestModName)!;
        using var _ = Assert.Multiple();
        await Assert.That(mod.State).IsEqualTo(ModState.Incompatible);
        await Assert.That(mod.IncompatibleReason).IsEqualTo(ModIncompatibleReason.GameVersion);
    }

    [Test]
    public async Task InitializeModsAsync_MelonVersionIncompatible_MarksIncompatible()
    {
        var sut = CreateModManageService(
            CreateGame(melonLoaderVersion: "0.5.0"),
            DownloadManagerWith(CreateWebMod(melonVersion: "0.6.0")),
            modLocalService: LocalServiceWith((TestModFilePath, CreateInstalledMod())));

        await sut.InitializeModsAsync();

        var mod = sut.FindModByName(TestModName)!;
        using var _ = Assert.Multiple();
        await Assert.That(mod.State).IsEqualTo(ModState.Incompatible);
        await Assert.That(mod.IncompatibleReason).IsEqualTo(ModIncompatibleReason.MelonLoader);
    }

    [Test]
    public async Task InitializeModsAsync_DuplicatedLocalMods_MarksDuplicated()
    {
        var sut = CreateModManageService(
            modLocalService: LocalServiceWith(
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
            CreateGame("1.0.0"),
            DownloadManagerWith(CreateWebMod("WebMod", gameVersion: "2.0.0")));

        await sut.InitializeModsAsync();

        var mod = sut.FindModByName("WebMod");
        using var _ = Assert.Multiple();
        await Assert.That(mod).IsNotNull();
        await Assert.That(mod!.IsLocal).IsFalse();
        await Assert.That(mod.State).IsEqualTo(ModState.Incompatible);
    }

    [Test]
    public async Task InitializeModsAsync_TwoInstalledMutuallyIncompatibleMods_MarksBothIncompatible()
    {
        var sut = CreateModManageService(
            gameDownloadManager: DownloadManagerWith(
                CreateWebMod("ModA", incompatibleMods: ["ModB"]),
                CreateWebMod("ModB")),
            modLocalService: LocalServiceWith(
                ("/mods/ModA.dll", CreateInstalledMod("ModA", "ModA.dll")),
                ("/mods/ModB.dll", CreateInstalledMod("ModB", "ModB.dll"))));

        await sut.InitializeModsAsync();

        var modA = sut.FindModByName("ModA")!;
        var modB = sut.FindModByName("ModB")!;
        using var _ = Assert.Multiple();
        await Assert.That(modA.State).IsEqualTo(ModState.Incompatible);
        await Assert.That(modB.State).IsEqualTo(ModState.Incompatible);
        await Assert.That(modA.IncompatibleReason).IsEqualTo(ModIncompatibleReason.ConflictingMod);
        await Assert.That(modA.ConflictingModNames).IsEquivalentTo(["ModB"], EqualityComparer<string>.Default, CollectionOrdering.Matching);
        await Assert.That(modB.ConflictingModNames).IsEquivalentTo(["ModA"], EqualityComparer<string>.Default, CollectionOrdering.Matching);
    }

    [Test]
    public async Task InitializeModsAsync_ConflictPartnerDisabled_DoesNotMarkIncompatible()
    {
        var sut = CreateModManageService(
            gameDownloadManager: DownloadManagerWith(
                CreateWebMod("ModA", incompatibleMods: ["ModB"]),
                CreateWebMod("ModB")),
            modLocalService: LocalServiceWith(
                ("/mods/ModA.dll", CreateInstalledMod("ModA", "ModA.dll")),
                ("/mods/ModB.disabled", CreateInstalledMod("ModB", "ModB.dll", disabled: true))));

        await sut.InitializeModsAsync();

        var modA = sut.FindModByName("ModA")!;
        var modB = sut.FindModByName("ModB")!;
        using var _ = Assert.Multiple();
        await Assert.That(modA.State).IsEqualTo(ModState.Normal);
        await Assert.That(modB.State).IsEqualTo(ModState.Normal);
        await Assert.That(modA.IncompatibleReason).IsEqualTo(ModIncompatibleReason.None);
        await Assert.That(modB.IncompatibleReason).IsEqualTo(ModIncompatibleReason.None);
    }

    [Test]
    public async Task InitializeModsAsync_WebModConflictsWithInstalled_MarksWebModIncompatible()
    {
        var sut = CreateModManageService(
            gameDownloadManager: DownloadManagerWith(
                CreateWebMod("ModA", incompatibleMods: ["ModB"]),
                CreateWebMod("ModB")),
            modLocalService: LocalServiceWith(("/mods/ModA.dll", CreateInstalledMod("ModA", "ModA.dll"))));

        await sut.InitializeModsAsync();

        var modB = sut.FindModByName("ModB")!;
        using var _ = Assert.Multiple();
        await Assert.That(sut.FindModByName("ModA")!.State).IsEqualTo(ModState.Normal);
        await Assert.That(modB.State).IsEqualTo(ModState.Incompatible);
        await Assert.That(modB.IncompatibleReason).IsEqualTo(ModIncompatibleReason.ConflictingMod);
        await Assert.That(modB.ConflictingModNames).IsEquivalentTo(["ModA"], EqualityComparer<string>.Default, CollectionOrdering.Matching);
        await Assert.That(modB.IsInstallable).IsFalse();
    }

    [Test]
    public async Task InitializeModsAsync_ConflictingModsBothUninstalled_DoesNotMarkIncompatible()
    {
        var sut = CreateModManageService(
            gameDownloadManager: DownloadManagerWith(
                CreateWebMod("ModA", incompatibleMods: ["ModB"]),
                CreateWebMod("ModB")));

        await sut.InitializeModsAsync();

        var modA = sut.FindModByName("ModA")!;
        var modB = sut.FindModByName("ModB")!;
        using var _ = Assert.Multiple();
        await Assert.That(modA.State).IsEqualTo(ModState.Normal);
        await Assert.That(modB.State).IsEqualTo(ModState.Normal);
        await Assert.That(modA.IsInstallable).IsTrue();
        await Assert.That(modB.IsInstallable).IsTrue();
    }

    [Test]
    public async Task InitializeModsAsync_IncompatibleModMissingFromCatalog_DoesNotMarkIncompatible()
    {
        var sut = CreateModManageService(
            gameDownloadManager: DownloadManagerWith(CreateWebMod("ModA", incompatibleMods: ["GhostMod"])),
            modLocalService: LocalServiceWith(("/mods/ModA.dll", CreateInstalledMod("ModA", "ModA.dll"))));

        await sut.InitializeModsAsync();

        await Assert.That(sut.FindModByName("ModA")!.State).IsEqualTo(ModState.Normal);
    }
}
