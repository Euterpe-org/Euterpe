namespace Euterpe.Tests.Core;

public sealed partial class GameShareServiceTest
{
    [Test]
    public async Task CreateInstalledModsShareLinkAsync_ThenParse_IncludesRemoteModsAndExcludesLocalOnly()
    {
        var modServiceMock = IModManageService.Mock();
        modServiceMock.GetInstalledMods().Returns([CreateRemoteMod("ModA", isDisabled: true), CreateLocalOnlyMod("Sideload")]);
        var service = CreateService(modManageService: modServiceMock);

        var parsed = service.TryParseShareLink((await service.CreateInstalledModsShareLinkAsync())!);

        using var _ = Assert.Multiple();
        await Assert.That(parsed).IsNotNull();
        await Assert.That(parsed!.ChartIds).IsEmpty();
        await Assert.That(parsed.Mods.Single().Name).IsEqualTo("ModA");
        await Assert.That(parsed.Mods.Single().IsDisabled).IsTrue();
    }

    [Test]
    public async Task CreateInstalledModsShareLinkAsync_NoDownloadableInstalledMods_ReturnsNull()
    {
        var modServiceMock = IModManageService.Mock();
        modServiceMock.GetInstalledMods().Returns([CreateLocalOnlyMod("Sideload")]);
        var service = CreateService(modManageService: modServiceMock);

        await Assert.That(await service.CreateInstalledModsShareLinkAsync()).IsNull();
    }
}
