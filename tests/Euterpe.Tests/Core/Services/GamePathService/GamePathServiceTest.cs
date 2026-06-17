using Euterpe.Models.VDFs;
using TUnit.Mocks.Logging;

namespace Euterpe.Tests.Core;

[Category("GamePathServiceTests")]
[TestSubject(typeof(GamePathService))]
public sealed partial class GamePathServiceTest
{
    private const string TestAppId = "774171";
    private const string TestRelativePath = "MuseDash";

    private readonly MockLogger<GamePathService> _logger = Mock.Logger<GamePathService>();
    private string _tempDir = null!;

    [Before(Test)]
    public void Setup()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"GamePathServiceTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    [After(Test)]
    public void Cleanup()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }

    private GamePathService CreateService(IVdfSerializationService vdf) =>
        new()
        {
            Config = new Config { SteamFolder = _tempDir, MuseDash = new MuseDashConfig(), MuseDash2 = new MuseDash2Config() },
            Logger = _logger,
            VdfSerializationService = vdf
        };

    private void CreateVdfFileMarker()
    {
        var steamApps = Path.Combine(_tempDir, "steamapps");
        Directory.CreateDirectory(steamApps);
        File.WriteAllText(Path.Combine(steamApps, "libraryfolders.vdf"), string.Empty);
    }

    private static IVdfSerializationService CreateVdfMockReturning(Dictionary<string, LibraryFolder> libraries)
    {
        var mock = IVdfSerializationService.Mock();
        mock.DeserializeFromFile<Dictionary<string, LibraryFolder>>(Any<string>()).Returns(libraries);
        return mock;
    }
}
