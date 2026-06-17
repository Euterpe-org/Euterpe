namespace Euterpe.Tests.Core;

public sealed partial class ModManageServiceTest
{
    [Test]
    public async Task ChangingMelonLoaderVersion_AfterInit_TransitionsCompatibleToIncompatible()
    {
        var game = CreateGame(melonLoaderVersion: "0.5.0");
        var sut = CreateModManageService(
            game,
            DownloadManagerWith(CreateWebMod(melonVersion: "0.5.0")),
            modLocalService: LocalServiceWith((TestModFilePath, CreateInstalledMod())));

        await sut.InitializeModsAsync();
        var mod = sut.FindModByName(TestModName);

        using var _ = Assert.Multiple();
        await Assert.That(mod!.State).IsEqualTo(ModState.Normal);

        game.MelonLoaderVersion = "0.4.0";
        await sut.ReconcileModsAsync();
        await Assert.That(mod.State).IsEqualTo(ModState.Incompatible);
    }

    [Test]
    public async Task ChangingMelonLoaderVersion_AfterInit_TransitionsIncompatibleToCompatible()
    {
        var game = CreateGame(melonLoaderVersion: "0.4.0");
        var sut = CreateModManageService(
            game,
            DownloadManagerWith(CreateWebMod(melonVersion: "0.5.0")),
            modLocalService: LocalServiceWith((TestModFilePath, CreateInstalledMod())));

        await sut.InitializeModsAsync();
        var mod = sut.FindModByName(TestModName);

        using var _ = Assert.Multiple();
        await Assert.That(mod!.State).IsEqualTo(ModState.Incompatible);

        game.MelonLoaderVersion = "0.5.0";
        await sut.ReconcileModsAsync();
        await Assert.That(mod.State).IsEqualTo(ModState.Normal);
    }

    [Test]
    public async Task ChangingMelonLoaderVersion_AfterInit_OutdatedModBecomesIncompatibleWhenUnsupported()
    {
        var game = CreateGame(melonLoaderVersion: "0.5.0");
        var sut = CreateModManageService(
            game,
            DownloadManagerWith(CreateWebMod(version: "2.0.0", melonVersion: "0.5.0")),
            modLocalService: LocalServiceWith((TestModFilePath, CreateInstalledMod())));

        await sut.InitializeModsAsync();
        var mod = sut.FindModByName(TestModName);

        await Assert.That(mod!.State).IsEqualTo(ModState.Outdated);

        game.MelonLoaderVersion = "0.4.0";
        await sut.ReconcileModsAsync();
        await Assert.That(mod.State).IsEqualTo(ModState.Incompatible);
    }

    [Test]
    public async Task ChangingMelonLoaderVersion_AfterInit_PreservesIncompatibleMods()
    {
        var game = CreateGame(melonLoaderVersion: "0.5.0");
        var sut = CreateModManageService(
            game,
            DownloadManagerWith(
                CreateWebMod("ModA", incompatibleMods: ["ModB"]),
                CreateWebMod("ModB")),
            modLocalService: LocalServiceWith(
                ("/mods/ModA.dll", CreateInstalledMod("ModA", "ModA.dll")),
                ("/mods/ModB.dll", CreateInstalledMod("ModB", "ModB.dll"))));

        await sut.InitializeModsAsync();
        game.MelonLoaderVersion = "0.6.0";
        await sut.ReconcileModsAsync();

        using var _ = Assert.Multiple();
        await Assert.That(sut.FindModByName("ModA")!.State).IsEqualTo(ModState.Incompatible);
        await Assert.That(sut.FindModByName("ModB")!.State).IsEqualTo(ModState.Incompatible);
    }
}
