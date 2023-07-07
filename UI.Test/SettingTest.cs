using System.Globalization;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Services;
using Serilog;
using Xunit.Abstractions;

namespace MuseDashModToolsUI.Test;

public class SettingTest
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

    private readonly ITestOutputHelper _testOutputHelper;

    public SettingTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task NullSettingTest()
    {
        await File.WriteAllTextAsync("Settings.json", SettingJson);
        var logService = new Mock<ILogger>();
        var messageBoxService = new Mock<IMessageBoxService>();
        var settingService = new SettingService(logService.Object) { MessageBoxService = messageBoxService.Object };
        await settingService.InitializeSettings();

        Assert.Equal(CultureInfo.CurrentUICulture.Name, settingService.Settings.LanguageCode);
        Assert.Equal("Segoe UI", settingService.Settings.FontName);
    }
}