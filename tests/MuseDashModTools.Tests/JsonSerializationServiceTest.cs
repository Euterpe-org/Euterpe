namespace MuseDashModTools.Tests;

[TestSubject(typeof(JsonSerializationService))]
public sealed class JsonSerializationServiceTest
{
    private const string ModJson = """
                                   {
                                       "Name": "TestMod",
                                       "Version": "1.0.0-beta",
                                       "Author": "test",
                                       "FileName": "Test Mod.dll",
                                       "Repository": "Org/Repo",
                                       "ConfigFile": "Config.cfg",
                                       "GameVersion": "*",
                                       "Description": "A Test Mod",
                                       "ModDependencies": [],
                                       "LibDependencies": [],
                                       "IncompatibleMods": [],
                                       "SHA256": "9fa86686c2a2f256d052e5319b0e7fded0d1ba9a95fd35113d499a28663b40e7"
                                   }
                                   """;

    private readonly JsonSerializationService _jsonSerializationService = new();

    private readonly Mod _mod = new()
    {
        Name = "TestMod",
        Version = "1.0.0-beta",
        Author = "test",
        FileName = "Test Mod.dll",
        Repository = "Org/Repo",
        ConfigFile = "Config.cfg",
        GameVersion = "*",
        Description = "A Test Mod",
        ModDependencies = [],
        LibDependencies = [],
        IncompatibleMods = [],
        SHA256 = "9fa86686c2a2f256d052e5319b0e7fded0d1ba9a95fd35113d499a28663b40e7"
    };
}