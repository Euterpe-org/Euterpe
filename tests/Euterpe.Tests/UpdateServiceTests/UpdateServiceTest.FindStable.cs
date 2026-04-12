using Semver;
using TUnit.Mocks.Logging;
using Ursa.Controls;

namespace Euterpe.Tests;

public sealed partial class UpdateServiceTest
{
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
            .WithHeader(ContentTypeHeader, AtomContentType);

        var updateService = CreateUpdateService();

        using var _ = Assert.Multiple();

        await Assert.That(await updateService.CheckForUpdatesAsync()).IsFalse();
        _logger.VerifyLog()
            .ContainingMessage($"Release version parsed: {LowerStableVersion}")
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
            .WithHeader(ContentTypeHeader, AtomContentType);

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
            .WithHeader(ContentTypeHeader, AtomContentType);

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
            .WithHeader(ContentTypeHeader, AtomContentType);

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
            .WithHeader(ContentTypeHeader, AtomContentType);

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
            .WithHeader(ContentTypeHeader, AtomContentType);

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
            .WithHeader(ContentTypeHeader, AtomContentType);

        var messageBoxServiceMock = IMessageBoxService.Mock();
        messageBoxServiceMock.NoticeConfirmAsync(Any<string>(), RefStructArg<ReadOnlySpan<object>>.Any)
            .Returns(MessageBoxResult.No);

        var updateService = CreateUpdateService(messageBoxService: messageBoxServiceMock);

        using var _ = Assert.Multiple();

        await Assert.That(await updateService.CheckForUpdatesAsync()).IsFalse();
        _logger.VerifyLog()
            .ContainingMessage($"User choose to skip this version: {HigherStableVersion}");
        await Assert.That(Config.SkipVersion).IsEqualTo(SemVersion.Parse(HigherStableVersion));
    }
}