using Euterpe.Contracts.Distribution;
using Euterpe.Contracts.Mods;
using TUnit.Mocks.Logging;

namespace Euterpe.Tests;

public sealed partial class ModManageServiceTest
{
    private const string TestGameFolder = "/games/MuseDash";
    private const string TestModName = "MyMod";
    private const string TestModFileName = "MyMod.dll";
    private const string TestModFilePath = "/mods/MyMod.dll";
    private const string TestLibName = "MyLib";
    private const string TestLibFileName = "MyLib.dll";
    private const string TestLibFilePath = "/libs/MyLib.dll";

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

    private static ModDto CreateInstallableMod(string name = TestModName, string fileName = TestModFileName) =>
        new()
        {
            Name = name,
            FileName = fileName,
            Version = "1.0.0"
        };

    private static ModDto CreateInstalledMod(string name = TestModName, string fileName = TestModFileName, bool disabled = false)
    {
        var mod = CreateInstallableMod(name, fileName);
        mod.AddLocalInfo();
        mod.IsDisabled = disabled;
        return mod;
    }

    private static Mod CreateWebMod(
        string name = TestModName,
        string version = "1.0.0",
        string sha256 = "",
        string gameVersion = "*",
        string melonVersion = "",
        string[]? libDependencies = null) =>
        new()
        {
            Name = name,
            Version = version,
            FileName = $"{name}.dll",
            GameVersion = gameVersion,
            MelonVersion = melonVersion,
            SHA256 = sha256,
            LibDependencies = libDependencies ?? []
        };

    private static Lib CreateWebLib(string slug = "MyLib", string sha256 = "") =>
        new()
        {
            Slug = slug,
            FileExtension = "dll",
            Versions =
            {
                ["1.0.0"] = new DistributionVersion<LibMetadata>
                {
                    SHA256 = sha256,
                    DownloadUrl = $"https://example.com/{slug}.dll"
                }
            }
        };

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

    private static ILocalService LocalServiceWith(params (string Path, ModDto Mod)[] mods)
    {
        var mock = ILocalService.Mock();
        mock.GetModFilePaths().Returns(mods.Select(m => m.Path).ToArray());
        mock.GetLibFilePaths().Returns([]);
        foreach (var (path, mod) in mods)
        {
            mock.LoadModFromPathAsync(path).Returns(mod);
        }

        return mock;
    }

    private static IDownloadManager DownloadManagerWith(params Mod[] webMods)
    {
        var mock = IDownloadManager.Mock();
        mock.FetchLibListAsync(Any<CancellationToken>()).Returns([]);
        mock.FetchModListAsync(Any<CancellationToken>()).Returns(webMods);
        return mock;
    }
}