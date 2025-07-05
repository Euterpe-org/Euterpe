using System.Text;
using MuseDashModTools.Models.Enums;

namespace MuseDashModTools.Tests;

[TestSubject(typeof(JsonSerializationService))]
public sealed class JsonSerializationServiceTest
{
    private const string ConfigJson = """
                                      {
                                          "MuseDashFolder": "SteamLibrary\\steamapps\\common\\Muse Dash",
                                          "CacheFolder": "Cache",
                                          "LanguageCode": "zh-Hans",
                                          "Theme": "Dark",
                                          "ShowConsole": true,
                                          "AlwaysShowScrollBar": true,
                                          "DownloadSource": "GitHub",
                                          "UpdateSource": "GitHubRSS",
                                          "GitHubToken": null,
                                          "CustomDownloadSource": null,
                                          "DownloadPrerelease": false,
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
            MuseDashFolder = "SteamLibrary\\steamapps\\common\\Muse Dash",
            CacheFolder = "Cache",
            LanguageCode = "zh-Hans",
            Theme = "Dark",
            ShowConsole = true,
            AlwaysShowScrollBar = true,
            DownloadSource = DownloadSource.GitHub,
            UpdateSource = UpdateSource.GitHubRSS,
            GitHubToken = null,
            CustomDownloadSource = null,
            DownloadPrerelease = false,
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