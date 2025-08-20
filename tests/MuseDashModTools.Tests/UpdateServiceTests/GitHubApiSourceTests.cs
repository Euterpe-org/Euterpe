using RichardSzalay.MockHttp;
using Semver;
using Ursa.Controls;

namespace MuseDashModTools.Tests.UpdateServiceTests;

[Category("UpdateServiceTests")]
[TestSubject(typeof(UpdateService))]
public sealed class GitHubApiSourceTests : UpdateServiceTestBase
{
    private const string ReleaseAPIUrl = "https://api.github.com/repos/MDModsDev/MuseDashModTools/releases";
    private const string LatestReleaseAPIUrl = "https://api.github.com/repos/MDModsDev/MuseDashModTools/releases/latest";

    protected override Config Config { get; } = new()
    {
        UpdateSource = UpdateSource.GitHubAPI
    };

    [Test]
    public async Task CheckForUpdatesAsync_FindStable_LowerStableVersion_ShouldNotFindUpdate()
    {
        MockHttp.When(LatestReleaseAPIUrl)
            .Respond("application/json",
                $$"""
                  {
                    "tag_name": "{{LowerStableVersion}}"
                  }
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
        MockHttp.When(ReleaseAPIUrl)
            .Respond("application/json",
                $$"""
                  [
                    {
                      "tag_name": "{{LowerPrereleaseVersion}}",
                      "prerelease": true
                    },
                    {
                      "tag_name": "{{LowerStableVersion}}",
                      "prerelease": false
                    }
                  ]
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
        MockHttp.When(LatestReleaseAPIUrl)
            .Respond("application/json",
                $$"""
                  {
                    "tag_name": "{{AppVersion}}"
                  }
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

        using var _ = Assert.Multiple();

        await Assert.That(await updateService.CheckForUpdatesAsync()).IsFalse();
        await Assert.That(TestContext.Current?.GetStandardOutput())
            .Contains($"Release version parsed: {AppVersion}")
            .And.Contains("No new version available");
    }

    [Test]
    public async Task CheckForUpdatesAsync_FindStable_LowerPrereleaseVersion_ShouldBeIgnoredAsPrerelease()
    {
        MockHttp.When(LatestReleaseAPIUrl)
            .Respond("application/json",
                $$"""
                  {
                    "tag_name": "{{LowerPrereleaseVersion}}"
                  }
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

        using var _ = Assert.Multiple();

        await Assert.That(await updateService.CheckForUpdatesAsync()).IsFalse();
        await Assert.That(TestContext.Current?.GetStandardOutput())
            .Contains($"Fetched stable release from GitHub API is a prerelease: {LowerPrereleaseVersion}");
    }

    [Test]
    public async Task CheckForUpdatesAsync_FindStable_HigherPrereleaseVersion_ShouldBeIgnoredAsPrerelease()
    {
        MockHttp.When(LatestReleaseAPIUrl)
            .Respond("application/json",
                $$"""
                  {
                    "tag_name": "{{HigherPrereleaseVersion}}"
                  }
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

        using var _ = Assert.Multiple();

        await Assert.That(await updateService.CheckForUpdatesAsync()).IsFalse();
        await Assert.That(TestContext.Current?.GetStandardOutput())
            .Contains($"Fetched stable release from GitHub API is a prerelease: {HigherPrereleaseVersion}");
    }

    [Test]
    [PrereleaseOnly]
    public async Task CheckForUpdatesAsync_FindStable_PrereleaseCurrentVersion_ShouldBeIgnoredAsPrerelease()
    {
        MockHttp.When(LatestReleaseAPIUrl)
            .Respond("application/json",
                $$"""
                  {
                    "tag_name": "{{AppVersion}}"
                  }
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

        using var _ = Assert.Multiple();

        await Assert.That(await updateService.CheckForUpdatesAsync()).IsFalse();
        await Assert.That(TestContext.Current?.GetStandardOutput())
            .Contains($"Fetched stable release from GitHub API is a prerelease: {AppVersion}");
    }

    [Test]
    public async Task CheckForUpdatesAsync_FindStable_WhenHigherStableVersionIsSkipped_ShouldSkipVersion()
    {
        Config.SkipVersion = SemVersion.Parse(HigherStableVersion);
        MockHttp.When(LatestReleaseAPIUrl)
            .Respond("application/json",
                $$"""
                  {
                    "tag_name": "{{HigherStableVersion}}"
                  }
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
        MockHttp.When(ReleaseAPIUrl)
            .Respond("application/json",
                $$"""
                  [
                    {
                      "tag_name": "{{HigherPrereleaseVersion}}",
                      "prerelease": true
                    },
                    {
                      "tag_name": "{{HigherStableVersion}}",
                      "prerelease": false
                    }
                  ]
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

        using var _ = Assert.Multiple();

        await Assert.That(await updateService.CheckForUpdatesAsync()).IsFalse();
        await Assert.That(TestContext.Current?.GetStandardOutput())
            .Contains("New version is skipped by user configuration");
    }

    [Test]
    public async Task CheckForUpdatesAsync_FindStable_WhenMessageBoxResultNo_ShouldSkipVersion()
    {
        MockHttp.When(LatestReleaseAPIUrl)
            .Respond("application/json",
                $$"""
                  {
                    "tag_name": "{{HigherStableVersion}}"
                  }
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
        MockHttp.When(ReleaseAPIUrl)
            .Respond("application/json",
                $$"""
                  [
                    {
                      "tag_name": "{{HigherPrereleaseVersion}}",
                      "prerelease": true
                    },
                    {
                      "tag_name": "{{HigherStableVersion}}",
                      "prerelease": false
                    }
                  ]
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

        using var _ = Assert.Multiple();

        await Assert.That(await updateService.CheckForUpdatesAsync()).IsFalse();
        await Assert.That(TestContext.Current?.GetStandardOutput())
            .Contains($"User choose to skip this version: {HigherPrereleaseVersion}");
        await Assert.That(Config.SkipVersion).IsEqualTo(SemVersion.Parse(HigherPrereleaseVersion));
    }
}