using RichardSzalay.MockHttp;
using Rocks;
using Semver;
using Ursa.Controls;

namespace Euterpe.Tests.UpdateServiceTests;

[Category("UpdateServiceTests")]
[TestSubject(typeof(UpdateService))]
public sealed class GitHubApiSourceTests : UpdateServiceTestBase
{
    private const string ReleaseAPIUrl = "https://api.github.com/repos/Euterpe-org/Euterpe/releases";
    private const string LatestReleaseAPIUrl = "https://api.github.com/repos/Euterpe-org/Euterpe/releases/latest";

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
        MockHttp.When(LatestReleaseAPIUrl)
            .Respond("application/json",
                $$"""
                  {
                    "tag_name": "{{AppVersion}}"
                  }
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
        MockHttp.When(LatestReleaseAPIUrl)
            .Respond("application/json",
                $$"""
                  {
                    "tag_name": "{{LowerPrereleaseVersion}}"
                  }
                  """);

        var updateService = CreateUpdateService();

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

        var updateService = CreateUpdateService();

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

        var updateService = CreateUpdateService();

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

        var updateService = CreateUpdateService();

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