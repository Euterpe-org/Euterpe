using RichardSzalay.MockHttp;
using Semver;
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

    private readonly TestLogger<UpdateService> _logger = new();
    private readonly MockHttpMessageHandler _mockHttp = new();
    private Config Config { get; } = new();

    public void Dispose()
    {
        _logger.Dispose();
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
            Client = _mockHttp.ToHttpClient(),
            Logger = _logger,
            DownloadManager = downloadManager ?? new IDownloadManagerMakeExpectations().Instance(),
            MessageBoxService = messageBoxService ?? new IMessageBoxServiceMakeExpectations().Instance(),
            PlatformService = platformService ?? new IPlatformServiceMakeExpectations().Instance()
        };

    [Test]
    public async Task CheckForUpdatesAsync_FindStable_LowerStableVersion_ShouldNotFindUpdate()
    {
        _mockHttp.When(TagsRSSUrl)
            .Respond("application/rss+xml",
                $"""
                 <?xml version="1.0" encoding="UTF-8"?>
                 <feed xmlns="http://www.w3.org/2005/Atom" xmlns:media="http://search.yahoo.com/mrss/" xml:lang="en-US">
                     <entry>
                         <title>{LowerStableVersion}</title>
                     </entry>
                 </feed>
                 """);

        var updateService = CreateUpdateService();

        using var _ = Assert.Multiple();

        await Assert.That(await updateService.CheckForUpdatesAsync()).IsFalse();
        await Assert.That(TestContext.Current?.GetStandardOutput())
            .Contains($"Release version parsed: {LowerStableVersion}")
            .And.Contains("No new version available");
    }

    [Test]
    public async Task CheckForUpdatesAsync_FindPrerelease_LowerPrereleaseVersion_ShouldNotFindUpdate()
    {
        Config.UpdateChannel = UpdateChannel.Prerelease;
        _mockHttp.When(TagsRSSUrl)
            .Respond("application/rss+xml",
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
                 """);

        var updateService = CreateUpdateService();

        using var _ = Assert.Multiple();

        await Assert.That(await updateService.CheckForUpdatesAsync()).IsFalse();
        await Assert.That(TestContext.Current?.GetStandardOutput())
            .Contains($"Release version parsed: {LowerPrereleaseVersion}")
            .And.Contains("No new version available");
    }

    [Test]
    [StableReleaseOnly]
    public async Task CheckForUpdatesAsync_FindStable_StableCurrentVersion_ShouldNotFindUpdate()
    {
        _mockHttp.When(TagsRSSUrl)
            .Respond("application/rss+xml",
                $"""
                 <?xml version="1.0" encoding="UTF-8"?>
                 <feed xmlns="http://www.w3.org/2005/Atom" xmlns:media="http://search.yahoo.com/mrss/" xml:lang="en-US">
                     <entry>
                         <title>{AppVersion}</title>
                     </entry>
                 </feed>
                 """);

        var updateService = CreateUpdateService();

        using var _ = Assert.Multiple();

        await Assert.That(await updateService.CheckForUpdatesAsync()).IsFalse();
        await Assert.That(TestContext.Current?.GetStandardOutput())
            .Contains($"Release version parsed: {AppVersion}")
            .And.Contains("No new version available");
    }

    [Test]
    public async Task CheckForUpdatesAsync_FindStable_LowerPrereleaseVersion_ShouldBeIgnoredAsPrerelease()
    {
        _mockHttp.When(TagsRSSUrl)
            .Respond("application/rss+xml",
                $"""
                 <?xml version="1.0" encoding="UTF-8"?>
                 <feed xmlns="http://www.w3.org/2005/Atom" xmlns:media="http://search.yahoo.com/mrss/" xml:lang="en-US">
                     <entry>
                         <title>{LowerPrereleaseVersion}</title>
                     </entry>
                 </feed>
                 """);

        var updateService = CreateUpdateService();

        using var _ = Assert.Multiple();

        await Assert.That(await updateService.CheckForUpdatesAsync()).IsFalse();
        await Assert.That(TestContext.Current?.GetStandardOutput())
            .Contains($"Fetched stable release from RSS is a prerelease: {LowerPrereleaseVersion}");
    }

    [Test]
    public async Task CheckForUpdatesAsync_FindStable_HigherPrereleaseVersion_ShouldBeIgnoredAsPrerelease()
    {
        _mockHttp.When(TagsRSSUrl)
            .Respond("application/rss+xml",
                $"""
                 <?xml version="1.0" encoding="UTF-8"?>
                 <feed xmlns="http://www.w3.org/2005/Atom" xmlns:media="http://search.yahoo.com/mrss/" xml:lang="en-US">
                     <entry>
                         <title>{HigherPrereleaseVersion}</title>
                     </entry>
                 </feed>
                 """);

        var updateService = CreateUpdateService();

        using var _ = Assert.Multiple();

        await Assert.That(await updateService.CheckForUpdatesAsync()).IsFalse();
        await Assert.That(TestContext.Current?.GetStandardOutput())
            .Contains($"Fetched stable release from RSS is a prerelease: {HigherPrereleaseVersion}");
    }

    [Test]
    [PrereleaseOnly]
    public async Task CheckForUpdatesAsync_FindStable_PrereleaseCurrentVersion_ShouldBeIgnoredAsPrerelease()
    {
        _mockHttp.When(TagsRSSUrl)
            .Respond("application/rss+xml",
                $"""
                 <?xml version="1.0" encoding="UTF-8"?>
                 <feed xmlns="http://www.w3.org/2005/Atom" xmlns:media="http://search.yahoo.com/mrss/" xml:lang="en-US">
                     <entry>
                         <title>{AppVersion}</title>
                     </entry>
                 </feed>
                 """);

        var updateService = CreateUpdateService();

        using var _ = Assert.Multiple();

        await Assert.That(await updateService.CheckForUpdatesAsync()).IsFalse();
        await Assert.That(TestContext.Current?.GetStandardOutput())
            .Contains($"Fetched stable release from RSS is a prerelease: {AppVersion}");
    }

    [Test]
    public async Task CheckForUpdatesAsync_FindStable_WhenHigherStableVersionIsSkipped_ShouldSkipVersion()
    {
        Config.SkipVersion = SemVersion.Parse(HigherStableVersion);
        _mockHttp.When(TagsRSSUrl)
            .Respond("application/rss+xml",
                $"""
                 <?xml version="1.0" encoding="UTF-8"?>
                 <feed xmlns="http://www.w3.org/2005/Atom" xmlns:media="http://search.yahoo.com/mrss/" xml:lang="en-US">
                     <entry>
                         <title>{HigherStableVersion}</title>
                     </entry>
                 </feed>
                 """);

        var updateService = CreateUpdateService();

        using var _ = Assert.Multiple();

        await Assert.That(await updateService.CheckForUpdatesAsync()).IsFalse();
        await Assert.That(TestContext.Current?.GetStandardOutput())
            .Contains("New version is skipped by user configuration");
    }

    [Test]
    public async Task CheckForUpdatesAsync_FindPrerelease_WhenHigherPrereleaseVersionIsSkipped_ShouldSkipVersion()
    {
        Config.UpdateChannel = UpdateChannel.Prerelease;
        Config.SkipVersion = SemVersion.Parse(HigherPrereleaseVersion);
        _mockHttp.When(TagsRSSUrl)
            .Respond("application/rss+xml",
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
                 """);

        var updateService = CreateUpdateService();

        using var _ = Assert.Multiple();

        await Assert.That(await updateService.CheckForUpdatesAsync()).IsFalse();
        await Assert.That(TestContext.Current?.GetStandardOutput())
            .Contains("New version is skipped by user configuration");
    }

    [Test]
    public async Task CheckForUpdatesAsync_FindStable_WhenMessageBoxResultNo_ShouldSkipVersion()
    {
        _mockHttp.When(TagsRSSUrl)
            .Respond("application/rss+xml",
                $"""
                 <?xml version="1.0" encoding="UTF-8"?>
                 <feed xmlns="http://www.w3.org/2005/Atom" xmlns:media="http://search.yahoo.com/mrss/" xml:lang="en-US">
                     <entry>
                         <title>{HigherStableVersion}</title>
                     </entry>
                 </feed>
                 """);

        var expectations = new IMessageBoxServiceCreateExpectations();
        expectations.Setups
            .NoticeConfirmAsync(Arg.Any<string>(), new RefStructArgument<ReadOnlySpan<object>>())
            .ReturnValue(Task.FromResult(MessageBoxResult.No));
        var messageBoxService = expectations.Instance();

        var updateService = CreateUpdateService(messageBoxService: messageBoxService);

        using var _ = Assert.Multiple();

        await Assert.That(await updateService.CheckForUpdatesAsync()).IsFalse();
        await Assert.That(TestContext.Current?.GetStandardOutput())
            .Contains($"User choose to skip this version: {HigherStableVersion}");
        await Assert.That(Config.SkipVersion).IsEqualTo(SemVersion.Parse(HigherStableVersion));
    }

    [Test]
    public async Task CheckForUpdatesAsync_FindPrerelease_WhenMessageBoxResultNo_ShouldSkipVersion()
    {
        Config.UpdateChannel = UpdateChannel.Prerelease;
        _mockHttp.When(TagsRSSUrl)
            .Respond("application/rss+xml",
                $"""
                 <?xml version="1.0" encoding="UTF-8"?>
                 <feed xmlns="http://www.w3.org/2005/Atom" xmlns:media="http://search.yahoo.com/mrss/" xml:lang="en-US">
                     <entry>
                         <title>{HigherPrereleaseVersion}</title>
                     </entry>
                 </feed>
                 """);

        var expectations = new IMessageBoxServiceCreateExpectations();
        expectations.Setups
            .NoticeConfirmAsync(Arg.Any<string>(), new RefStructArgument<ReadOnlySpan<object>>())
            .ReturnValue(Task.FromResult(MessageBoxResult.No));
        var messageBoxService = expectations.Instance();

        var updateService = CreateUpdateService(messageBoxService: messageBoxService);

        using var _ = Assert.Multiple();

        await Assert.That(await updateService.CheckForUpdatesAsync()).IsFalse();
        await Assert.That(TestContext.Current?.GetStandardOutput())
            .Contains($"User choose to skip this version: {HigherPrereleaseVersion}");
        await Assert.That(Config.SkipVersion).IsEqualTo(SemVersion.Parse(HigherPrereleaseVersion));
    }
}