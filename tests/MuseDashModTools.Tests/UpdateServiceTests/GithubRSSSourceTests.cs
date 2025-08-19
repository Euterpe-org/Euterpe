using RichardSzalay.MockHttp;
using Semver;
using Ursa.Controls;

namespace MuseDashModTools.Tests.UpdateServiceTests;

[Category("UpdateServiceTests")]
[TestSubject(typeof(UpdateService))]
public sealed class GithubRSSSourceTests : UpdateServiceTestBase
{
    private const string TagsRSSUrl = "https://github.com/MDModsDev/MuseDashModTools/releases.atom";

    protected override Config Config { get; } = new()
    {
        UpdateSource = UpdateSource.GitHubRSS
    };

    [Test]
    public async Task CheckForUpdatesAsync_FindStable_LowerStableVersion_ShouldNotFindUpdate()
    {
        MockHttp.When(TagsRSSUrl)
            .Respond("application/rss+xml",
                $"""
                 <?xml version="1.0" encoding="UTF-8"?>
                 <feed xmlns="http://www.w3.org/2005/Atom" xmlns:media="http://search.yahoo.com/mrss/" xml:lang="en-US">
                     <entry>
                         <title>{LowerStableVersion}</title>
                     </entry>
                 </feed>
                 """);

        var updateService = new UpdateService
        {
            Config = Config,
            Client = MockHttp.ToHttpClient(),
            Logger = Logger,
            DownloadManager = Mock.Of<IDownloadManager>(),
            MessageBoxService = Mock.Of<IMessageBoxService>(),
            PlatformService = Mock.Of<IPlatformService>()
        };

        await updateService.CheckForUpdatesAsync();

        await Assert.That(TestContext.Current?.GetStandardOutput())
            .Contains($"Release version parsed: {LowerStableVersion}")
            .Contains("No new version available");
    }

    [Test]
    public async Task CheckForUpdatesAsync_FindPrerelease_LowerPrereleaseVersion_ShouldNotFindUpdate()
    {
        Config.DownloadPrerelease = true;
        MockHttp.When(TagsRSSUrl)
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

        var updateService = new UpdateService
        {
            Config = Config,
            Client = MockHttp.ToHttpClient(),
            Logger = Logger,
            DownloadManager = Mock.Of<IDownloadManager>(),
            MessageBoxService = Mock.Of<IMessageBoxService>(),
            PlatformService = Mock.Of<IPlatformService>()
        };

        await updateService.CheckForUpdatesAsync();

        await Assert.That(TestContext.Current?.GetStandardOutput())
            .Contains($"Release version parsed: {LowerPrereleaseVersion}")
            .Contains("No new version available");
    }

    [Test]
    [StableReleaseOnly]
    public async Task CheckForUpdatesAsync_FindStable_StableCurrentVersion_ShouldNotFindUpdate()
    {
        MockHttp.When(TagsRSSUrl)
            .Respond("application/rss+xml",
                $"""
                 <?xml version="1.0" encoding="UTF-8"?>
                 <feed xmlns="http://www.w3.org/2005/Atom" xmlns:media="http://search.yahoo.com/mrss/" xml:lang="en-US">
                     <entry>
                         <title>{AppVersion}</title>
                     </entry>
                 </feed>
                 """);

        var updateService = new UpdateService
        {
            Config = Config,
            Client = MockHttp.ToHttpClient(),
            Logger = Logger,
            DownloadManager = Mock.Of<IDownloadManager>(),
            MessageBoxService = Mock.Of<IMessageBoxService>(),
            PlatformService = Mock.Of<IPlatformService>()
        };

        await updateService.CheckForUpdatesAsync();

        await Assert.That(TestContext.Current?.GetStandardOutput())
            .Contains($"Release version parsed: {AppVersion}")
            .Contains("No new version available");
    }

    [Test]
    public async Task CheckForUpdatesAsync_FindStable_LowerPrereleaseVersion_ShouldBeIgnoredAsPrerelease()
    {
        MockHttp.When(TagsRSSUrl)
            .Respond("application/rss+xml",
                $"""
                 <?xml version="1.0" encoding="UTF-8"?>
                 <feed xmlns="http://www.w3.org/2005/Atom" xmlns:media="http://search.yahoo.com/mrss/" xml:lang="en-US">
                     <entry>
                         <title>{LowerPrereleaseVersion}</title>
                     </entry>
                 </feed>
                 """);

        var updateService = new UpdateService
        {
            Config = Config,
            Client = MockHttp.ToHttpClient(),
            Logger = Logger,
            DownloadManager = Mock.Of<IDownloadManager>(),
            MessageBoxService = Mock.Of<IMessageBoxService>(),
            PlatformService = Mock.Of<IPlatformService>()
        };

        await updateService.CheckForUpdatesAsync();

        await Assert.That(TestContext.Current?.GetStandardOutput())
            .Contains($"Fetched stable release from GitHub RSS is a prerelease: {LowerPrereleaseVersion}");
    }

    [Test]
    public async Task CheckForUpdatesAsync_FindStable_HigherPrereleaseVersion_ShouldBeIgnoredAsPrerelease()
    {
        MockHttp.When(TagsRSSUrl)
            .Respond("application/rss+xml",
                $"""
                 <?xml version="1.0" encoding="UTF-8"?>
                 <feed xmlns="http://www.w3.org/2005/Atom" xmlns:media="http://search.yahoo.com/mrss/" xml:lang="en-US">
                     <entry>
                         <title>{HigherPrereleaseVersion}</title>
                     </entry>
                 </feed>
                 """);

        var updateService = new UpdateService
        {
            Config = Config,
            Client = MockHttp.ToHttpClient(),
            Logger = Logger,
            DownloadManager = Mock.Of<IDownloadManager>(),
            MessageBoxService = Mock.Of<IMessageBoxService>(),
            PlatformService = Mock.Of<IPlatformService>()
        };

        await updateService.CheckForUpdatesAsync();

        await Assert.That(TestContext.Current?.GetStandardOutput())
            .Contains($"Fetched stable release from GitHub RSS is a prerelease: {HigherPrereleaseVersion}");
    }

    [Test]
    [PrereleaseOnly]
    public async Task CheckForUpdatesAsync_FindStable_PrereleaseCurrentVersion_ShouldBeIgnoredAsPrerelease()
    {
        MockHttp.When(TagsRSSUrl)
            .Respond("application/rss+xml",
                $"""
                 <?xml version="1.0" encoding="UTF-8"?>
                 <feed xmlns="http://www.w3.org/2005/Atom" xmlns:media="http://search.yahoo.com/mrss/" xml:lang="en-US">
                     <entry>
                         <title>{AppVersion}</title>
                     </entry>
                 </feed>
                 """);

        var updateService = new UpdateService
        {
            Config = Config,
            Client = MockHttp.ToHttpClient(),
            Logger = Logger,
            DownloadManager = Mock.Of<IDownloadManager>(),
            MessageBoxService = Mock.Of<IMessageBoxService>(),
            PlatformService = Mock.Of<IPlatformService>()
        };

        await updateService.CheckForUpdatesAsync();

        await Assert.That(TestContext.Current?.GetStandardOutput())
            .Contains($"Fetched stable release from GitHub RSS is a prerelease: {AppVersion}");
    }

    [Test]
    public async Task CheckForUpdatesAsync_FindStable_WhenHigherStableVersionIsSkipped_ShouldSkipVersion()
    {
        Config.SkipVersion = SemVersion.Parse(HigherStableVersion);
        MockHttp.When(TagsRSSUrl)
            .Respond("application/rss+xml",
                $"""
                 <?xml version="1.0" encoding="UTF-8"?>
                 <feed xmlns="http://www.w3.org/2005/Atom" xmlns:media="http://search.yahoo.com/mrss/" xml:lang="en-US">
                     <entry>
                         <title>{HigherStableVersion}</title>
                     </entry>
                 </feed>
                 """);

        var updateService = new UpdateService
        {
            Config = Config,
            Client = MockHttp.ToHttpClient(),
            Logger = Logger,
            DownloadManager = Mock.Of<IDownloadManager>(),
            MessageBoxService = Mock.Of<IMessageBoxService>(),
            PlatformService = Mock.Of<IPlatformService>()
        };

        await updateService.CheckForUpdatesAsync();

        await Assert.That(TestContext.Current?.GetStandardOutput())
            .Contains("New version is skipped by user configuration");
    }

    [Test]
    public async Task CheckForUpdatesAsync_FindPrerelease_WhenHigherPrereleaseVersionIsSkipped_ShouldSkipVersion()
    {
        Config.DownloadPrerelease = true;
        Config.SkipVersion = SemVersion.Parse(HigherPrereleaseVersion);
        MockHttp.When(TagsRSSUrl)
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

        var updateService = new UpdateService
        {
            Config = Config,
            Client = MockHttp.ToHttpClient(),
            Logger = Logger,
            DownloadManager = Mock.Of<IDownloadManager>(),
            MessageBoxService = Mock.Of<IMessageBoxService>(),
            PlatformService = Mock.Of<IPlatformService>()
        };

        await updateService.CheckForUpdatesAsync();

        await Assert.That(TestContext.Current?.GetStandardOutput())
            .Contains("New version is skipped by user configuration");
    }

    [Test]
    public async Task CheckForUpdatesAsync_FindStable_WhenMessageBoxResultNo_ShouldSkipVersion()
    {
        MockHttp.When(TagsRSSUrl)
            .Respond("application/rss+xml",
                $"""
                 <?xml version="1.0" encoding="UTF-8"?>
                 <feed xmlns="http://www.w3.org/2005/Atom" xmlns:media="http://search.yahoo.com/mrss/" xml:lang="en-US">
                     <entry>
                         <title>{HigherStableVersion}</title>
                     </entry>
                 </feed>
                 """);

        var messageBoxService = Mock.Of<IMessageBoxService>(m =>
            m.NoticeConfirmAsync(It.IsAny<string>()) == Task.FromResult(MessageBoxResult.No));

        var updateService = new UpdateService
        {
            Config = Config,
            Client = MockHttp.ToHttpClient(),
            Logger = Logger,
            DownloadManager = Mock.Of<IDownloadManager>(),
            MessageBoxService = messageBoxService,
            PlatformService = Mock.Of<IPlatformService>()
        };

        await updateService.CheckForUpdatesAsync();

        await Assert.That(TestContext.Current?.GetStandardOutput())
            .Contains($"User choose to skip this version: {HigherStableVersion}");

        await Assert.That(Config.SkipVersion).IsEqualTo(SemVersion.Parse(HigherStableVersion));
    }

    [Test]
    public async Task CheckForUpdatesAsync_FindPrerelease_WhenMessageBoxResultNo_ShouldSkipVersion()
    {
        Config.DownloadPrerelease = true;
        MockHttp.When(TagsRSSUrl)
            .Respond("application/rss+xml",
                $"""
                 <?xml version="1.0" encoding="UTF-8"?>
                 <feed xmlns="http://www.w3.org/2005/Atom" xmlns:media="http://search.yahoo.com/mrss/" xml:lang="en-US">
                     <entry>
                         <title>{HigherPrereleaseVersion}</title>
                     </entry>
                 </feed>
                 """);

        var messageBoxService = Mock.Of<IMessageBoxService>(m =>
            m.NoticeConfirmAsync(It.IsAny<string>()) == Task.FromResult(MessageBoxResult.No));

        var updateService = new UpdateService
        {
            Config = Config,
            Client = MockHttp.ToHttpClient(),
            Logger = Logger,
            DownloadManager = Mock.Of<IDownloadManager>(),
            MessageBoxService = messageBoxService,
            PlatformService = Mock.Of<IPlatformService>()
        };

        await updateService.CheckForUpdatesAsync();

        await Assert.That(TestContext.Current?.GetStandardOutput())
            .Contains($"User choose to skip this version: {HigherPrereleaseVersion}");

        await Assert.That(Config.SkipVersion).IsEqualTo(SemVersion.Parse(HigherPrereleaseVersion));
    }
}