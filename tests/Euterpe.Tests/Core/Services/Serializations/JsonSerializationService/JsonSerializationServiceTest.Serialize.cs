namespace Euterpe.Tests;

public sealed partial class JsonSerializationServiceTest
{
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
        MuseDash2 = new MuseDash2Config(),
        LanguageCode = "zh-Hans",
        Theme = "Dark",
        ShowConsole = true,
        AlwaysShowScrollBar = true,
        UpdateChannel = UpdateChannel.Stable,
        SkipVersion = null,
        IgnoreException = false
    };

    [Test]
    public Task SerializeConfig_ShouldReturnValidJson()
    {
        var stream = new MemoryStream();
        _jsonSerializationService.SerializeConfig(stream, CreateTestConfig());
        stream.Position = 0;
        return VerifyJson(stream);
    }

    [Test]
    public async Task SerializeConfigAsync_ShouldReturnValidJson()
    {
        var stream = new MemoryStream();
        await _jsonSerializationService.SerializeConfigAsync(stream, CreateTestConfig());
        stream.Position = 0;
        await VerifyJson(stream);
    }
}
