using System.Globalization;
using System.IO.Abstractions;

namespace MuseDashModToolsUI.Test;

public class SavingServiceTest(ITestOutputHelper testOutputHelper)
{
    private const string SettingJson = """
                                       {
                                        "MuseDashFolder": "MuseDash",
                                        "LanguageCode": null,
                                        "FontName": null,
                                        "SkipVersion": null,
                                        "DownloadPrerelease": false,
                                        "DownloadSource": 0,
                                        "CustomDownloadSource": null,
                                        "Theme": "Light",
                                        "ShowConsole": false,
                                        "AskInstallMuseDashModTools": 0,
                                        "AskEnableDepWhenInstall": 0,
                                        "AskEnableDepWhenEnable": 0,
                                        "AskDisableDepWhenDelete": 0,
                                        "AskDisableDepWhenDisable": 0
                                       }
                                       """;

    private readonly TestLogger _logger = new(testOutputHelper);

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
        fs.Setup(f => f.File.ReadAllText(It.IsAny<string>())).Returns(SettingJson);
        var savingService = new SavingService
        {
            FileSystem = fs.Object,
            FileSystemPickerService = new Mock<IFileSystemPickerService>().Object,
            Logger = _logger,
            LocalService = new Mock<ILocalService>().Object,
            MessageBoxService = new Mock<IMessageBoxService>().Object,
            PlatformService = new Mock<IPlatformService>().Object,
            SerializationService = new SerializationService(),
            UpdateUIService = new Mock<IUpdateUIService>().Object
        };

        savingService.LoadSettings();
        await savingService.InitializeSettingsAsync();

        Assert.Equal(CultureInfo.CurrentUICulture.Name, savingService.Settings.LanguageCode);
        Assert.Equal(FontManageService.DefaultFont, savingService.Settings.FontName);
    }
}