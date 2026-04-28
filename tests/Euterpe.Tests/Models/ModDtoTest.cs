using Euterpe.Contracts.Mods;

namespace Euterpe.Tests;

[Category("ModDtoTests")]
[TestSubject(typeof(ModDto))]
public sealed class ModDtoTest
{
    private static ModDto Create(string fileName = "MyMod.dll", string? localFnWithoutExt = null, bool disabled = true) =>
        new()
        {
            Name = "MyMod",
            FileName = fileName,
            FileNameWithoutExtension = localFnWithoutExt,
            IsDisabled = disabled,
            Version = "1.0.0"
        };

    [Test]
    public async Task AddLocalInfo_PromotesToLocalAndCopiesVersion()
    {
        var mod = Create();

        mod.AddLocalInfo();

        using var _ = Assert.Multiple();
        await Assert.That(mod.LocalVersion).IsEqualTo("1.0.0");
        await Assert.That(mod.State).IsEqualTo(ModState.Normal);
        await Assert.That(mod.FileNameWithoutExtension).IsEqualTo("MyMod");
        await Assert.That(mod.IsDisabled).IsFalse();
        await Assert.That(mod.IsLocal).IsTrue();
    }

    [Test]
    public async Task RemoveLocalInfo_DemotesToWebOnly()
    {
        var mod = Create(localFnWithoutExt: "MyMod", disabled: false);
        mod.LocalVersion = "1.0.0";
        mod.State = ModState.Normal;

        mod.RemoveLocalInfo();

        using var _ = Assert.Multiple();
        await Assert.That(mod.LocalVersion).IsEmpty();
        await Assert.That(mod.State).IsEqualTo(ModState.Normal);
        await Assert.That(mod.FileNameWithoutExtension).IsNull();
        await Assert.That(mod.IsDisabled).IsTrue();
        await Assert.That(mod.IsLocal).IsFalse();
    }

    [Test]
    [Arguments(true, "MyMod.disabled", "MyMod.dll")]
    [Arguments(false, "MyMod.dll", "MyMod.disabled")]
    public async Task LocalAndReversedFileName_DependOnIsDisabledFlag(bool isDisabled, string expectedLocal, string expectedReversed)
    {
        var mod = Create(localFnWithoutExt: "MyMod", disabled: isDisabled);

        using var _ = Assert.Multiple();
        await Assert.That(mod.LocalFileName).IsEqualTo(expectedLocal);
        await Assert.That(mod.ReversedFileName).IsEqualTo(expectedReversed);
    }

    [Test]
    [Arguments(null, false)]
    [Arguments("MyMod", true)]
    public async Task IsLocal_ReflectsFileNameWithoutExtensionPresence(string? fileNameWithoutExt, bool expected)
    {
        var mod = Create(localFnWithoutExt: fileNameWithoutExt);
        await Assert.That(mod.IsLocal).IsEqualTo(expected);
    }

    [Test]
    [Arguments("", false)]
    [Arguments("MyMod.dll", true)]
    public async Task HasDownloadSource_ReflectsFileNamePresence(string fileName, bool expected)
    {
        var mod = Create(fileName);
        await Assert.That(mod.HasDownloadSource).IsEqualTo(expected);
    }

    public static IEnumerable<Func<(bool isLocal, bool hasDownload, ModState state, bool expected)>> InstallableCases()
    {
        // not local + has download + not incompatible → installable
        yield return () => (false, true, ModState.Normal, true);
        yield return () => (false, true, ModState.Outdated, true);
        // local mods are never installable
        yield return () => (true, true, ModState.Normal, false);
        // no download source → not installable
        yield return () => (false, false, ModState.Normal, false);
        // incompatible → not installable
        yield return () => (false, true, ModState.Incompatible, false);
    }

    [Test]
    [MethodDataSource(nameof(InstallableCases))]
    public async Task IsInstallable_DependsOnLocalDownloadAndStateFlags((bool isLocal, bool hasDownload, ModState state, bool expected) data)
    {
        var mod = Create(
            fileName: data.hasDownload ? "MyMod.dll" : "",
            localFnWithoutExt: data.isLocal ? "MyMod" : null);
        mod.State = data.state;

        await Assert.That(mod.IsInstallable).IsEqualTo(data.expected);
    }

    [Test]
    [Arguments(true, ModState.Modified, true)]
    [Arguments(true, ModState.Normal, false)]
    [Arguments(true, ModState.Outdated, false)]
    [Arguments(false, ModState.Modified, false)]
    public async Task IsReinstallable_RequiresLocalAndModifiedState(bool isLocal, ModState state, bool expected)
    {
        var mod = Create(localFnWithoutExt: isLocal ? "MyMod" : null);
        mod.State = state;
        await Assert.That(mod.IsReinstallable).IsEqualTo(expected);
    }

    [Test]
    public async Task HasDependency_TrueWhenAnyDependenciesExist()
    {
        var mod = Create();

        using var _ = Assert.Multiple();
        await Assert.That(mod.HasDependency).IsFalse();

        mod.ModDependencies = ["DepA"];
        await Assert.That(mod.HasDependency).IsTrue();

        mod.ModDependencies = [];
        mod.LibDependencies = ["LibA"];
        await Assert.That(mod.HasDependency).IsTrue();
    }

    [Test]
    public async Task DependencyNames_ReturnsModAndLibDependenciesCombined()
    {
        var mod = Create();
        mod.ModDependencies = ["ModA", "ModB"];
        mod.LibDependencies = ["LibX"];

        await Assert.That(mod.DependencyNames.SequenceEqual(["ModA", "ModB", "LibX"], StringComparer.Ordinal)).IsTrue();
    }

    [Test]
    public async Task DependencyNames_EmptyWhenNoDependencies()
    {
        var mod = Create();
        await Assert.That(mod.DependencyNames).IsEmpty();
    }

    [Test]
    public async Task HasScreenshots_TrueWhenScreenshotsPresent()
    {
        var mod = Create();

        using var _ = Assert.Multiple();
        await Assert.That(mod.HasScreenshots).IsFalse();

        mod.Screenshots = [new ModScreenshot { Url = "https://example.com/img.png" }];
        await Assert.That(mod.HasScreenshots).IsTrue();
    }
}
