using TUnit.Mocks.Logging;

namespace Euterpe.Tests;

public sealed partial class ModManageServiceTest
{
    private const string TestGameFolder = "/games/MuseDash";
    private const string TestModName = "MyMod";

    private readonly MockLogger<ModManageService> _logger = Mock.Logger<ModManageService>();

    private Config Config { get; } = new() { MuseDashFolder = TestGameFolder };

    private ModManageService CreateModManageService(
        Config? config = null,
        IDownloadManager? downloadManager = null,
        IFileSystemService? fileSystemService = null,
        ILocalService? localService = null,
        INotificationService? notificationService = null) =>
        new()
        {
            Config = config ?? Config,
            Logger = _logger,
            DownloadManager = downloadManager ?? CreateEmptyDownloadManager(),
            FileSystemService = fileSystemService ?? IFileSystemService.Mock(),
            LocalService = localService ?? CreateEmptyLocalService(),
            NotificationService = notificationService ?? INotificationService.Mock()
        };

    private static ModDto CreateInstallableMod(string name = TestModName, string fileName = "MyMod.dll") =>
        new()
        {
            Name = name,
            FileName = fileName,
            Version = "1.0.0"
        };

    private static ModDto CreateInstalledMod(string name = TestModName, string fileName = "MyMod.dll", bool disabled = false)
    {
        var mod = CreateInstallableMod(name, fileName);
        mod.AddLocalInfo();
        mod.IsDisabled = disabled;
        return mod;
    }

    private static IDownloadManager CreateEmptyDownloadManager()
    {
        var mock = IDownloadManager.Mock();
        mock.FetchModListAsync(Any<CancellationToken>()).Returns([]);
        mock.FetchLibListAsync(Any<CancellationToken>()).Returns([]);
        return mock;
    }

    private static ILocalService CreateEmptyLocalService()
    {
        var mock = ILocalService.Mock();
        mock.GetModFilePaths().Returns([]);
        mock.GetLibFilePaths().Returns([]);
        return mock;
    }
}