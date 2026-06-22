using System.Text;

namespace Euterpe.Tests.Core;

[Category("JsonSerializationServiceTests")]
[TestSubject(typeof(JsonSerializationService))]
public sealed partial class JsonSerializationServiceTest
{
    private const string ConfigJson = """
                                      {
                                          "SteamFolder": "C:\\Program Files (x86)\\SteamLibrary",
                                          "SteamExecPath": "C:\\Program Files (x86)\\SteamLibrary\\steam.exe",
                                          "CacheFolder": "Cache",
                                          "ActiveGame": "MuseDash",
                                          "MuseDash": {
                                              "Folder": "C:\\Program Files (x86)\\SteamLibrary\\steamapps\\common\\Muse Dash",
                                              "GameMode": "Vanilla"
                                          },
                                          "MuseDash2": {
                                              "Folder": "",
                                              "GameMode": "Modded"
                                          },
                                          "LanguageCode": "zh-Hans",
                                          "Theme": "Dark",
                                          "ShowConsole": true,
                                          "AlwaysShowScrollBar": true,
                                          "DownloadSource": "GitHub",
                                          "UpdateSource": "GitHubRSS",
                                          "GitHubToken": null,
                                          "UpdateChannel": "Stable",
                                          "IgnoreException": false
                                      }
                                      """;

    private readonly JsonSerializationService _jsonSerializationService = new();

    [Test]
    public Task DeserializeConfig_ShouldReturnValidConfig()
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(ConfigJson));
        var config = _jsonSerializationService.DeserializeConfig(stream);

        return Verify(config);
    }

    [Test]
    public async Task DeserializeConfigAsync_ShouldReturnValidConfig()
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(ConfigJson));
        var config = await _jsonSerializationService.DeserializeConfigAsync(stream);

        await Verify(config);
    }
}
