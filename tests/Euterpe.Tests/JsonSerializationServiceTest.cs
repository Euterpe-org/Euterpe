using System.Text;

namespace Euterpe.Tests;

[Category("JsonSerializationServiceTests")]
[TestSubject(typeof(JsonSerializationService))]
public sealed class JsonSerializationServiceTest
{
    private const string ConfigJson = """
                                      {
                                          "SteamFolder": "C:\\Program Files (x86)\\SteamLibrary",
                                          "SteamExecPath": "C:\\Program Files (x86)\\SteamLibrary\\steam.exe",
                                          "MuseDashFolder": "C:\\Program Files (x86)\\SteamLibrary\\steamapps\\common\\Muse Dash",
                                          "CacheFolder": "Cache",
                                          "GameMode": "Vanilla",
                                          "LanguageCode": "zh-Hans",
                                          "Theme": "Dark",
                                          "ShowConsole": true,
                                          "AlwaysShowScrollBar": true,
                                          "DownloadSource": "GitHub",
                                          "UpdateSource": "GitHubRSS",
                                          "GitHubToken": null,
                                          "UpdateChannel": "Stable",
                                          "SkipVersion": null,
                                          "IgnoreException": false
                                      }
                                      """;

    private readonly JsonSerializationService _jsonSerializationService = new();

    [Test]
    public async Task SerializeConfig_ShouldReturnValidJson()
    {
        var config = new Config
        {
            SteamFolder = @"C:\Program Files (x86)\SteamLibrary",
            SteamExecPath = @"C:\Program Files (x86)\SteamLibrary\steam.exe",
            MuseDashFolder = @"C:\Program Files (x86)\SteamLibrary\steamapps\common\Muse Dash",
            CacheFolder = "Cache",
            GameMode = GameMode.Vanilla,
            LanguageCode = "zh-Hans",
            Theme = "Dark",
            ShowConsole = true,
            AlwaysShowScrollBar = true,
            DownloadSource = DownloadSource.GitHub,
            UpdateSource = UpdateSource.GitHubRSS,
            GitHubToken = null,
            UpdateChannel = UpdateChannel.Stable,
            SkipVersion = null,
            IgnoreException = false
        };

        var stream = new MemoryStream();
        await _jsonSerializationService.SerializeConfigAsync(stream, config);
        stream.Position = 0;
        await VerifyJson(stream);
    }

    [Test]
    public async Task DeserializeConfig_ShouldReturnValidConfig()
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(ConfigJson));
        var config = await _jsonSerializationService.DeserializeConfigAsync(stream);

        await Verify(config);
    }
}