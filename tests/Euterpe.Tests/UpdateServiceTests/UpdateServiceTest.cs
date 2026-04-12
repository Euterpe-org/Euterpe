using TUnit.Mocks.Http;
using TUnit.Mocks.Logging;

namespace Euterpe.Tests;

public sealed partial class UpdateServiceTest : IDisposable
{
    private const string AppVersion = BuildInfo.AppVersion;
    private const string ContentTypeHeader = "Content-Type";
    private const string AtomContentType = "application/rss+xml";
    private const string LowerStableVersion = "0.0.1";
    private const string LowerPrereleaseVersion = "0.0.1-rc1";
    private const string HigherStableVersion = "999.0.0";
    private const string HigherPrereleaseVersion = "999.0.1-rc1";
    private const string TagsRSSUrl = "https://releases.euterpe-org.com/releases.atom";
    private readonly MockLogger<UpdateService> _logger = Mock.Logger<UpdateService>();
    private readonly MockHttpHandler _mockHttp = new();
    private Config Config { get; } = new();

    public void Dispose()
    {
        _mockHttp.Dispose();
    }

    private UpdateService CreateUpdateService(
        Config? config = null,
        IDownloadManager? downloadManager = null,
        IMessageBoxService? messageBoxService = null,
        IPlatformService? platformService = null) =>
        new()
        {
            Config = config ?? Config,
            Client = _mockHttp.CreateClient(),
            Logger = _logger,
            DownloadManager = downloadManager ?? IDownloadManager.Mock(),
            MessageBoxService = messageBoxService ?? IMessageBoxService.Mock(),
            PlatformService = platformService ?? IPlatformService.Mock()
        };
}