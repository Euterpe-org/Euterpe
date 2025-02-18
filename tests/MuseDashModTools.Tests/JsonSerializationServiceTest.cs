using System.Text;

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
                                       "GameVersion": "*",
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
        FileName = "Test Mod.dll",
        GameVersion = "*",
        IncompatibleMods = [],
        Name = "TestMod",
        Repository = "Org/Repo",
        SHA256 = "9fa86686c2a2f256d052e5319b0e7fded0d1ba9a95fd35113d499a28663b40e7",
        Version = "1.0.0-beta"
    };

    [Test]
    public async Task DeserializeTest()
    {
        var result = _jsonSerializationService.Deserialize<Mod>(ModJson);
        await Verify(result);
    }

    [Test]
    public async Task DeserializeIndentedTest()
    {
        var result = _jsonSerializationService.DeserializeIndented<Mod>(ModJson);
        await Verify(result);
    }

    [Test]
    public async Task DeserializeAsyncTest()
    {
        var memoryStream = new MemoryStream();
        await memoryStream.WriteAsync(Encoding.UTF8.GetBytes(ModJson).AsMemory());
        memoryStream.Position = 0;
        var result = await _jsonSerializationService.DeserializeAsync<Mod>(memoryStream);
        await Verify(result);
    }

    [Test]
    public async Task SerializeTest()
    {
        var result = _jsonSerializationService.Serialize(_mod);
        await Verify(result);
    }

    [Test]
    public async Task SerializeAsyncTest()
    {
        var memoryStream = new MemoryStream();
        await _jsonSerializationService.SerializeAsync(memoryStream, _mod);
        memoryStream.Position = 0;
        var result = await new StreamReader(memoryStream).ReadToEndAsync();
        await Verify(result);
    }

    [Test]
    public async Task SerializeIndentedTest()
    {
        var result = _jsonSerializationService.SerializeIndented(_mod);
        await Verify(result);
    }
}