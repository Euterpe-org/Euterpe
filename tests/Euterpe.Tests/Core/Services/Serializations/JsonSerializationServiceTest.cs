namespace Euterpe.Tests;

[Category("JsonSerializationServiceTests")]
[TestSubject(typeof(JsonSerializationService))]
public sealed partial class JsonSerializationServiceTest
{
    private const string ConfigJson = """
                                      {
                                          "SteamFolder": "C:\\Program Files (x86)\\SteamLibrary",
                                          "SteamExecPath": "C:\\Program Files (x86)\\SteamLibrary\\steam.exe",
                                          "CacheFolder": "Cache",
                                          "MuseDash": {
                                              "Folder": "C:\\Program Files (x86)\\SteamLibrary\\steamapps\\common\\Muse Dash",
                                              "GameMode": "Vanilla"
                                          },
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

    private static Config CreateTestConfig() => new()
    {
        SteamFolder = @"C:\Program Files (x86)\SteamLibrary",
        SteamExecPath = @"C:\Program Files (x86)\SteamLibrary\steam.exe",
        CacheFolder = "Cache",
        MuseDash = new MuseDashConfig
        {
            Folder = @"C:\Program Files (x86)\SteamLibrary\steamapps\common\Muse Dash",
            GameMode = GameMode.Vanilla,
            GameVersion = "1.0.0",
            UnityVersion = "2019.4.32",
            MelonLoaderVersion = "0.6.5"
        },
        LanguageCode = "zh-Hans",
        Theme = "Dark",
        ShowConsole = true,
        AlwaysShowScrollBar = true,
        UpdateChannel = UpdateChannel.Stable,
        SkipVersion = null,
        IgnoreException = false
    };
}