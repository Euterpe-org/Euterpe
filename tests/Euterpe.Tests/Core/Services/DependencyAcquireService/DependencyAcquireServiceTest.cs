using Euterpe.Contracts.Distribution;
using Euterpe.Core.Http.Clients;
using Euterpe.Core.Utils;
using TUnit.Mocks.Logging;

namespace Euterpe.Tests.Core;

[Category("DependencyAcquireServiceTests")]
[TestSubject(typeof(DependencyAcquireService))]
public sealed partial class DependencyAcquireServiceTest
{
    private const string TestUnityVersion = "2019.4.32";
    private const string TestMelonLoaderVersion = "0.6.5";
    private const string TestDotNetRuntimeVersion = "6.0";

    private readonly MockLogger<DependencyAcquireService> _logger = Mock.Logger<DependencyAcquireService>();
    private MuseDashConfig _game = null!;
    private string _tempDir = null!;

    [Before(Test)]
    public void Setup()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"DependencyAcquireServiceTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _game = new MuseDashConfig { Folder = _tempDir };
    }

    [After(Test)]
    public void Cleanup()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }

    private DependencyAcquireService CreateService(
        IEuterpeDistributionClient? distributionClient = null,
        IAppDownloadManager? appDownloadManager = null) =>
        new()
        {
            GameConfig = _game,
            DistributionClient = distributionClient ?? IEuterpeDistributionClient.Mock(),
            AppDownloadManager = appDownloadManager ?? IAppDownloadManager.Mock(),
            Logger = _logger
        };

    private static Dependency CreateDependency(
        string slug,
        string version,
        string sha256,
        string downloadUrl = "https://example.com/file",
        string dotNetRuntimeVersion = "") =>
        new()
        {
            Slug = slug,
            FileExtension = "zip",
            Versions = new Dictionary<string, DistributionVersion<DependencyMetadata>>
            {
                [version] = new()
                {
                    SHA256 = sha256,
                    DownloadUrl = downloadUrl,
                    FileSize = 100,
                    Metadata = new DependencyMetadata { DotNetRuntimeVersion = dotNetRuntimeVersion }
                }
            }
        };

    private static Dependency[] CreateAllMelonLoaderDeps(string sha) =>
    [
        CreateDependency("MelonLoader", TestMelonLoaderVersion, sha, dotNetRuntimeVersion: TestDotNetRuntimeVersion),
        CreateDependency("UnityDependencies", TestUnityVersion, sha),
        CreateDependency("Cpp2IL", "2024.1.0", sha),
        CreateDependency("Cpp2IL-Plugin", "1.0.0", sha)
    ];

    private static IEuterpeDistributionClient CreateClientReturning(params Dependency[] deps)
    {
        var mock = IEuterpeDistributionClient.Mock();
        mock.GetLatestDependenciesAsync(true, Any<CancellationToken>()).Returns(deps);
        return mock;
    }

    private async Task<string> CreateValidDependencyFiles()
    {
        var content = "test-content"u8.ToArray();
        string[] paths = [_game.MelonLoaderZipPath, _game.UnityDependencyZipPath(TestUnityVersion), _game.Cpp2ILExecutablePath, _game.Cpp2ILPluginPath];

        foreach (var path in paths)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await File.WriteAllBytesAsync(path, content);
        }

        return await SHA256Utils.HexLowerFromPathAsync(paths[0]);
    }
}
