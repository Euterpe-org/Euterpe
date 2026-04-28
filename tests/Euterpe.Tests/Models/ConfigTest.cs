using Semver;

namespace Euterpe.Tests;

[Category("ConfigTests")]
[TestSubject(typeof(Config))]
public sealed class ConfigTest
{
    private const string GameFolder = "/games/MuseDash";

    [Test]
    public async Task ComputedPaths_DerivedFromMuseDashFolder()
    {
        var config = new Config { MuseDashFolder = GameFolder };

        using var _ = Assert.Multiple();
        await Assert.That(config.ModsFolder).IsEqualTo(Path.Combine(GameFolder, "Mods"));
        await Assert.That(config.UserDataFolder).IsEqualTo(Path.Combine(GameFolder, "UserData"));
        await Assert.That(config.UserLibsFolder).IsEqualTo(Path.Combine(GameFolder, "UserLibs"));
        await Assert.That(config.MelonLoaderFolder).IsEqualTo(Path.Combine(GameFolder, "MelonLoader"));
        await Assert.That(config.MelonLoaderZipPath).IsEqualTo(Path.Combine(GameFolder, "MelonLoader.zip"));
        await Assert.That(config.LatestLogPath).IsEqualTo(Path.Combine(GameFolder, "MelonLoader", "Latest.log"));
    }

    [Test]
    public async Task ChartsFolders_NestedUnderEuterpeChartsFolder()
    {
        var config = new Config { MuseDashFolder = GameFolder };

        using var _ = Assert.Multiple();
        await Assert.That(config.OnlineChartsFolder).IsEqualTo(Path.Combine(GameFolder, "Euterpe_Charts", "Online"));
        await Assert.That(config.OfflineChartsFolder).IsEqualTo(Path.Combine(GameFolder, "Euterpe_Charts", "Offline"));
    }

    [Test]
    public async Task UnityDependencyZipPath_IncludesUnityVersion()
    {
        var config = new Config { MuseDashFolder = GameFolder, UnityVersion = "2019.4.32" };

        await Assert.That(config.UnityDependencyZipPath).IsEqualTo(
            Path.Combine(GameFolder, "MelonLoader", "Dependencies", "Il2CppAssemblyGenerator", "UnityDependencies_2019.4.32.zip"));
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments("not-a-version")]
    public async Task SettingMelonLoaderVersion_InvalidString_LeavesSemVersionNull(string? input)
    {
        var config = new Config { MelonLoaderVersion = input };
        await Assert.That(config.MelonLoaderSemVersion).IsNull();
    }

    [Test]
    [Arguments("0.5.7")]
    [Arguments("1.0.0")]
    [Arguments("0.6.0-rc1")]
    public async Task SettingMelonLoaderVersion_ValidString_ParsesSemVersion(string input)
    {
        var config = new Config { MelonLoaderVersion = input };

        using var _ = Assert.Multiple();
        await Assert.That(config.MelonLoaderSemVersion).IsNotNull();
        await Assert.That(config.MelonLoaderSemVersion).IsEqualTo(SemVersion.Parse(input));
    }

    [Test]
    public async Task DefaultGameMode_IsModded() =>
        await Assert.That(new Config().GameMode).IsEqualTo(GameMode.Modded);

    [Test]
    public async Task DefaultUpdateChannel_IsStable() =>
        await Assert.That(new Config().UpdateChannel).IsEqualTo(UpdateChannel.Stable);
}