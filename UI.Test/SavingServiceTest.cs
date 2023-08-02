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
        fs.Setup(f => f.File.Exists(It.IsAny<string>())).Returns(true);
        fs.Setup(f => f.File.ReadAllTextAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()).Result).Returns(SettingJson);
        var savingService = new SavingService(_logger, fs.Object) { MessageBoxService = new Mock<IMessageBoxService>().Object };
        await savingService.InitializeSettings();

        Assert.Equal(CultureInfo.CurrentUICulture.Name, savingService.Settings.LanguageCode);
        Assert.Equal("Segoe UI", savingService.Settings.FontName);
    }
}