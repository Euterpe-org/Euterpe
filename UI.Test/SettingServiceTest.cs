using System.Globalization;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Services;
using Xunit.Abstractions;

namespace MuseDashModToolsUI.Test;

public class SettingServiceTest
{
    private const string SettingJson = """
                                       {
                                        "MuseDashFolder": "MuseDash",
                                        "LanguageCode": null,
                                        "FontName": null,
                                        "DownloadSource": 0,
                                        "AskInstallMuseDashModTools": 0,
                                        "AskEnableDependenciesWhenInstalling": 0,
                                        "AskEnableDependenciesWhenEnabling": 0,
                                        "AskDisableDependenciesWhenDeleting": 0,
                                        "AskDisableDependenciesWhenDisabling": 0
                                       }
                                       """;

    private readonly TestLogger _logger;

    public SettingServiceTest(ITestOutputHelper testOutputHelper)
    {
        _logger = new TestLogger(testOutputHelper);
    }

    [Fact]
    public async Task NullSettingTest()
    {
        await File.WriteAllTextAsync("Settings.json", SettingJson);
        var settingService = new SettingService(_logger) { MessageBoxService = new Mock<IMessageBoxService>().Object };
        await settingService.InitializeSettings();

        Assert.Equal(CultureInfo.CurrentUICulture.Name, settingService.Settings.LanguageCode);
        Assert.Equal("Segoe UI", settingService.Settings.FontName);
    }
}