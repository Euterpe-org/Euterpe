using System.Globalization;
using System.IO.Abstractions;

namespace MuseDashModToolsUI.Test;

public class SavingServiceTest
{
    private const string SettingJson = """
                                       {
                                        "MuseDashFolder": "MuseDash",
                                        "LanguageCode": null,
                                        "FontName": null,
                                        "DownloadSource": 0,
                                        "DownloadPrerelease": false,
                                        "AskInstallMuseDashModTools": 0,
                                        "AskEnableDependenciesWhenInstalling": 0,
                                        "AskEnableDependenciesWhenEnabling": 0,
                                        "AskDisableDependenciesWhenDeleting": 0,
                                        "AskDisableDependenciesWhenDisabling": 0
                                       }
                                       """;

    private readonly TestLogger _logger;

    public SavingServiceTest(ITestOutputHelper testOutputHelper) => _logger = new TestLogger(testOutputHelper);

    [Fact]
    public async Task NullSettingTest()
    {
        var fs = new Mock<IFileSystem>();
        var settingPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MuseDashModTools",
            "Settings.json");
        var updaterPath = Path.Combine(Directory.GetCurrentDirectory(), "Update", "Updater.exe");
        fs.Setup(f => f.File.Exists(settingPath)).Returns(true);
        fs.Setup(f => f.File.Exists(updaterPath)).Returns(false);
        fs.Setup(f => f.Directory.Exists(It.IsAny<string?>())).Returns(false);
        fs.Setup(f => f.File.ReadAllTextAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(SettingJson);
        var savingService = new SavingService(_logger, fs.Object) { MessageBoxService = new Mock<IMessageBoxService>().Object };
        await savingService.InitializeSettings();

        Assert.Equal(CultureInfo.CurrentUICulture.Name, savingService.Settings.LanguageCode);
        Assert.Equal(FontManageService.DefaultFont, savingService.Settings.FontName);
    }
}