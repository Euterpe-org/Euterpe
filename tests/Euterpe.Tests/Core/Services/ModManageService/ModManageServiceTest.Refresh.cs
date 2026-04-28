namespace Euterpe.Tests;

public sealed partial class ModManageServiceTest
{
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