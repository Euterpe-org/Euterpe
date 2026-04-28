using Euterpe.Contracts.Distribution;
using Euterpe.Core.Http.Clients;
using Semver;
using TUnit.Mocks.Logging;

namespace Euterpe.Tests;

public sealed partial class UpdateServiceTest
{
    private const string CurrentStableVersion = "1.0.0";
    private const string CurrentPrereleaseVersion = "1.0.0-rc1";
    private const string LowerStableVersion = "0.0.1";
    private const string LowerPrereleaseVersion = "0.0.1-rc1";
    private const string HigherStableVersion = "999.0.0";
    private const string HigherPrereleaseVersion = "999.0.1-rc1";
    private const string TestRuntimeIdentifier = "test-rid";
    private const string TestDownloadUrl = "https://example.com/release.zip";

    private readonly MockLogger<UpdateService> _logger = Mock.Logger<UpdateService>();

    private Config Config { get; } = new() { MuseDash = new MuseDashConfig() };

    private UpdateService CreateUpdateService(
        Config? config = null,
        SemVersion? currentVersion = null,
        IEuterpeDistributionClient? distributionClient = null,
        IDownloadManager? downloadManager = null,
        IMessageBoxService? messageBoxService = null,
        IPlatformService? platformService = null)
    {
        var platformServiceMock = platformService ?? CreatePlatformServiceMock();

        return new UpdateService
        {
            Config = config ?? Config,
            CurrentVersion = currentVersion ?? SemVersion.Parse(CurrentStableVersion),
            Logger = _logger,
            DistributionClient = distributionClient ?? IEuterpeDistributionClient.Mock(),
            DownloadManager = downloadManager ?? IDownloadManager.Mock(),
            MessageBoxService = messageBoxService ?? IMessageBoxService.Mock(),
            PlatformService = platformServiceMock
        };
    }

    private static IPlatformService CreatePlatformServiceMock()
    {
        var mock = IPlatformService.Mock();
        mock.RuntimeIdentifier.Returns(TestRuntimeIdentifier);
        return mock;
    }

    private static IEuterpeDistributionClient CreateDistributionClientMock(string version)
    {
        var release = new Release
        {
            Slug = TestRuntimeIdentifier,
            FileExtension = "zip",
            Versions = new Dictionary<string, DistributionVersion<ReleaseMetadata>>
            {
                [version] = new()
                {
                    DownloadUrl = TestDownloadUrl,
                    SHA256 = "sha256",
                    FileSize = 100
                }
            }
        };

        var mock = IEuterpeDistributionClient.Mock();
        mock.GetAppReleaseAsync(Any<bool>(), Any<bool>(), Any<CancellationToken>())
            .Returns([release]);
        return mock;
    }
}