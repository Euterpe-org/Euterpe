using Semver;
using TUnit.Mocks.Http;
using TUnit.Mocks.Logging;
using Ursa.Controls;

namespace Euterpe.Tests;

public sealed class UpdateServiceTest : IDisposable
{
    private const string AppVersion = BuildInfo.AppVersion;
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
            DownloadManager = downloadManager ?? Mock.Of<IDownloadManager>().Object,
            MessageBoxService = messageBoxService ?? Mock.Of<IMessageBoxService>().Object,
            PlatformService = platformService ?? Mock.Of<IPlatformService>().Object
        };

    [Test]
    public async Task CheckForUpdatesAsync_FindStable_LowerStableVersion_ShouldNotFindUpdate()
    {
        _mockHttp.OnGet(TagsRSSUrl)
            .RespondWithString(
                $"""
                 <?xml version="1.0" encoding="UTF-8"?>
                 <feed xmlns="http://www.w3.org/2005/Atom" xmlns:media="http://search.yahoo.com/mrss/" xml:lang="en-US">
                     <entry>
                         <title>{LowerStableVersion}</title>
                     </entry>
                 </feed>
                 """)
            .WithHeader("Content-Type", "application/rss+xml");

        var updateService = CreateUpdateService();

        using var _ = Assert.Multiple();

        await Assert.That(await updateService.CheckForUpdatesAsync()).IsFalse();
        _logger.VerifyLog()
            .ContainingMessage($"Release version parsed: {LowerStableVersion}")
            .ContainingMessage("No new version available");
    }

    [Test]
    public async Task CheckForUpdatesAsync_FindPrerelease_LowerPrereleaseVersion_ShouldNotFindUpdate()
    {
        Config.UpdateChannel = UpdateChannel.Prerelease;
        _mockHttp.OnGet(TagsRSSUrl)
            .RespondWithString(
                $"""
                 <?xml version="1.0" encoding="UTF-8"?>
                 <feed xmlns="http://www.w3.org/2005/Atom" xmlns:media="http://search.yahoo.com/mrss/" xml:lang="en-US">
                     <entry>
                        <title>{LowerPrereleaseVersion}</title>
                     </entry>
                     <entry>
                        <title>{LowerStableVersion}</title>
                     </entry>
                 </feed>
                 """)
            .WithHeader("Content-Type", "application/rss+xml");

        var updateService = CreateUpdateService();

        using var _ = Assert.Multiple();

        await Assert.That(await updateService.CheckForUpdatesAsync()).IsFalse();
        _logger.VerifyLog()
            .ContainingMessage($"Release version parsed: {LowerPrereleaseVersion}")
            .ContainingMessage("No new version available");
    }

    [Test]
    [StableReleaseOnly]
    public async Task CheckForUpdatesAsync_FindStable_StableCurrentVersion_ShouldNotFindUpdate()
    {
        _mockHttp.OnGet(TagsRSSUrl)
            .RespondWithString(
                $"""
                 <?xml version="1.0" encoding="UTF-8"?>
                 <feed xmlns="http://www.w3.org/2005/Atom" xmlns:media="http://search.yahoo.com/mrss/" xml:lang="en-US">
                     <entry>
                         <title>{AppVersion}</title>
                     </entry>
                 </feed>
                 """)
            .WithHeader("Content-Type", "application/rss+xml");

        var updateService = CreateUpdateService();

        using var _ = Assert.Multiple();

        await Assert.That(await updateService.CheckForUpdatesAsync()).IsFalse();
        _logger.VerifyLog()
            .ContainingMessage($"Release version parsed: {AppVersion}")
            .ContainingMessage("No new version available");
    }

    [Test]
    public async Task CheckForUpdatesAsync_FindStable_LowerPrereleaseVersion_ShouldBeIgnoredAsPrerelease()
    {
        _mockHttp.OnGet(TagsRSSUrl)
            .RespondWithString(
                $"""
                 <?xml version="1.0" encoding="UTF-8"?>
                 <feed xmlns="http://www.w3.org/2005/Atom" xmlns:media="http://search.yahoo.com/mrss/" xml:lang="en-US">
                     <entry>
                         <title>{LowerPrereleaseVersion}</title>
                     </entry>
                 </feed>
                 """)
            .WithHeader("Content-Type", "application/rss+xml");

        var updateService = CreateUpdateService();

        using var _ = Assert.Multiple();

        await Assert.That(await updateService.CheckForUpdatesAsync()).IsFalse();
        _logger.VerifyLog()
            .ContainingMessage($"Fetched stable release from RSS is a prerelease: {LowerPrereleaseVersion}");
    }

    [Test]
    public async Task CheckForUpdatesAsync_FindStable_HigherPrereleaseVersion_ShouldBeIgnoredAsPrerelease()
    {
        _mockHttp.OnGet(TagsRSSUrl)
            .RespondWithString(
                $"""
                 <?xml version="1.0" encoding="UTF-8"?>
                 <feed xmlns="http://www.w3.org/2005/Atom" xmlns:media="http://search.yahoo.com/mrss/" xml:lang="en-US">
                     <entry>
                         <title>{HigherPrereleaseVersion}</title>
                     </entry>
                 </feed>
                 """)
            .WithHeader("Content-Type", "application/rss+xml");

        var updateService = CreateUpdateService();

        using var _ = Assert.Multiple();

        await Assert.That(await updateService.CheckForUpdatesAsync()).IsFalse();
        _logger.VerifyLog()
            .ContainingMessage($"Fetched stable release from RSS is a prerelease: {HigherPrereleaseVersion}");
    }

    [Test]
    [PrereleaseOnly]
    public async Task CheckForUpdatesAsync_FindStable_PrereleaseCurrentVersion_ShouldBeIgnoredAsPrerelease()
    {
        _mockHttp.OnGet(TagsRSSUrl)
            .RespondWithString(
                $"""
                 <?xml version="1.0" encoding="UTF-8"?>
                 <feed xmlns="http://www.w3.org/2005/Atom" xmlns:media="http://search.yahoo.com/mrss/" xml:lang="en-US">
                     <entry>
                         <title>{AppVersion}</title>
                     </entry>
                 </feed>
                 """)
            .WithHeader("Content-Type", "application/rss+xml");

        var updateService = CreateUpdateService();

        using var _ = Assert.Multiple();

        await Assert.That(await updateService.CheckForUpdatesAsync()).IsFalse();
        _logger.VerifyLog()
            .ContainingMessage($"Fetched stable release from RSS is a prerelease: {AppVersion}");
    }

    [Test]
    public async Task CheckForUpdatesAsync_FindStable_WhenHigherStableVersionIsSkipped_ShouldSkipVersion()
    {
        Config.SkipVersion = SemVersion.Parse(HigherStableVersion);
        _mockHttp.OnGet(TagsRSSUrl)
            .RespondWithString(
                $"""
                 <?xml version="1.0" encoding="UTF-8"?>
                 <feed xmlns="http://www.w3.org/2005/Atom" xmlns:media="http://search.yahoo.com/mrss/" xml:lang="en-US">
                     <entry>
                         <title>{HigherStableVersion}</title>
                     </entry>
                 </feed>
                 """)
            .WithHeader("Content-Type", "application/rss+xml");

        var updateService = CreateUpdateService();

        using var _ = Assert.Multiple();

        await Assert.That(await updateService.CheckForUpdatesAsync()).IsFalse();
        _logger.VerifyLog()
            .ContainingMessage("New version is skipped by user configuration");
    }

    [Test]
    public async Task CheckForUpdatesAsync_FindPrerelease_WhenHigherPrereleaseVersionIsSkipped_ShouldSkipVersion()
    {
        Config.UpdateChannel = UpdateChannel.Prerelease;
        Config.SkipVersion = SemVersion.Parse(HigherPrereleaseVersion);
        _mockHttp.OnGet(TagsRSSUrl)
            .RespondWithString(
                $"""
                 <?xml version="1.0" encoding="UTF-8"?>
                 <feed xmlns="http://www.w3.org/2005/Atom" xmlns:media="http://search.yahoo.com/mrss/" xml:lang="en-US">
                     <entry>
                        <title>{HigherPrereleaseVersion}</title>
                     </entry>
                     <entry>
                        <title>{HigherStableVersion}</title>
                     </entry>
                 </feed>
                 """)
            .WithHeader("Content-Type", "application/rss+xml");

        var updateService = CreateUpdateService();

        using var _ = Assert.Multiple();

        await Assert.That(await updateService.CheckForUpdatesAsync()).IsFalse();
        _logger.VerifyLog()
            .ContainingMessage("New version is skipped by user configuration");
    }

    [Test]
    public async Task CheckForUpdatesAsync_FindStable_WhenMessageBoxResultNo_ShouldSkipVersion()
    {
        _mockHttp.OnGet(TagsRSSUrl)
            .RespondWithString(
                $"""
                 <?xml version="1.0" encoding="UTF-8"?>
                 <feed xmlns="http://www.w3.org/2005/Atom" xmlns:media="http://search.yahoo.com/mrss/" xml:lang="en-US">
                     <entry>
                         <title>{HigherStableVersion}</title>
                     </entry>
                 </feed>
                 """)
            .WithHeader("Content-Type", "application/rss+xml");

        var messageBoxServiceMock = Mock.Of<IMessageBoxService>();
        messageBoxServiceMock.NoticeConfirmAsync(Any<string>(), RefStructArg<ReadOnlySpan<object>>.Any)
            .Returns(MessageBoxResult.No);

        var updateService = CreateUpdateService(messageBoxService: messageBoxServiceMock.Object);

        using var _ = Assert.Multiple();

        await Assert.That(await updateService.CheckForUpdatesAsync()).IsFalse();
        _logger.VerifyLog()
            .ContainingMessage($"User choose to skip this version: {HigherStableVersion}");
        await Assert.That(Config.SkipVersion).IsEqualTo(SemVersion.Parse(HigherStableVersion));
    }

    [Test]
    public async Task CheckForUpdatesAsync_FindPrerelease_WhenMessageBoxResultNo_ShouldSkipVersion()
    {
        Config.UpdateChannel = UpdateChannel.Prerelease;
        _mockHttp.OnGet(TagsRSSUrl)
            .RespondWithString(
                $"""
                 <?xml version="1.0" encoding="UTF-8"?>
                 <feed xmlns="http://www.w3.org/2005/Atom" xmlns:media="http://search.yahoo.com/mrss/" xml:lang="en-US">
                     <entry>
                         <title>{HigherPrereleaseVersion}</title>
                     </entry>
                 </feed>
                 """)
            .WithHeader("Content-Type", "application/rss+xml");

        var messageBoxServiceMock = Mock.Of<IMessageBoxService>();
        messageBoxServiceMock.NoticeConfirmAsync(Any<string>(), RefStructArg<ReadOnlySpan<object>>.Any)
            .Returns(MessageBoxResult.No);

        var updateService = CreateUpdateService(messageBoxService: messageBoxServiceMock.Object);

        using var _ = Assert.Multiple();

        await Assert.That(await updateService.CheckForUpdatesAsync()).IsFalse();
        _logger.VerifyLog()
            .ContainingMessage($"User choose to skip this version: {HigherPrereleaseVersion}");
        await Assert.That(Config.SkipVersion).IsEqualTo(SemVersion.Parse(HigherPrereleaseVersion));
    }
}