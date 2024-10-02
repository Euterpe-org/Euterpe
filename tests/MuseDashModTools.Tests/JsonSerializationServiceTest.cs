namespace MuseDashModTools.Tests;

[TestSubject(typeof(JsonSerializationService))]
public sealed class JsonSerializationServiceTest
{
    private const string ModJson = """
                                   {
                                       "Name": "TestMod",
                                       "Version": "1.0.0-beta",
                                       "Author": "test",
                                       "DownloadLink": "Test Mod.dll",
                                       "RepositoryIdentifier": "Org/Repo",
                                       "ConfigFile": "Config.cfg",
                                       "GameVersion": [
                                         "*"
                                       ],
                                       "Description": "A Test Mod",
                                       "DependentMods": [],
                                       "DependentLibs": [],
                                       "IncompatibleMods": [],
                                       "SHA256": "9fa86686c2a2f256d052e5319b0e7fded0d1ba9a95fd35113d499a28663b40e7"
                                   }
                                   """;

    private readonly JsonSerializationService _jsonSerializationService = new();

    private readonly Mod _mod = new()
    {
        Author = "test",
        ConfigFile = "Config.cfg",
        DependentLibs = [],
        DependentMods = [],
        Description = "A Test Mod",
        DownloadLink = "Test Mod.dll",
        GameVersion = ["*"],
        IncompatibleMods = [],
        Name = "TestMod",
        RepositoryIdentifier = "Org/Repo",
        SHA256 = "9fa86686c2a2f256d052e5319b0e7fded0d1ba9a95fd35113d499a28663b40e7",
        Version = "1.0.0-beta"
    };

    [Test]
    public async Task DeserializeTest()
    {
        var result = _jsonSerializationService.Deserialize<Mod>(ModJson);
        await AssertResult(result);
    }

    private async Task AssertResult(Mod? mod)
    {
        await Assert.That(mod).IsNotNull();
        await Assert.That(_mod.Author).IsEqualTo(mod.Author);
        await Assert.That(_mod.ConfigFile).IsEqualTo(mod.ConfigFile);
        await Assert.That(_mod.DependentLibs).IsEquivalentTo(mod.DependentLibs);
        await Assert.That(_mod.DependentMods).IsEquivalentTo(mod.DependentMods);
        await Assert.That(_mod.Description).IsEqualTo(mod.Description);
        await Assert.That(_mod.DownloadLink).IsEqualTo(mod.DownloadLink);
        await Assert.That(_mod.GameVersion).IsEquivalentTo(mod.GameVersion);
        await Assert.That(_mod.IncompatibleMods).IsEquivalentTo(mod.IncompatibleMods);
        await Assert.That(_mod.Name).IsEqualTo(mod.Name);
        await Assert.That(_mod.RepositoryIdentifier).IsEqualTo(mod.RepositoryIdentifier);
        await Assert.That(_mod.SHA256).IsEqualTo(mod.SHA256);
        await Assert.That(_mod.Version).IsEqualTo(mod.Version);
    }
}