using Semver;
using TUnit.Mocks.Logging;
using Ursa.Controls;

namespace Euterpe.Tests;

public sealed partial class UpdateServiceTest
{
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
            .WithHeader(ContentTypeHeader, AtomContentType);

        var updateService = CreateUpdateService();

        using var _ = Assert.Multiple();

        await Assert.That(await updateService.CheckForUpdatesAsync()).IsFalse();
        _logger.VerifyLog()
            .ContainingMessage($"Release version parsed: {LowerPrereleaseVersion}")
            .ContainingMessage("No new version available");
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
            .WithHeader(ContentTypeHeader, AtomContentType);

        var updateService = CreateUpdateService();

        using var _ = Assert.Multiple();

        await Assert.That(await updateService.CheckForUpdatesAsync()).IsFalse();
        _logger.VerifyLog()
            .ContainingMessage("New version is skipped by user configuration");
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
            .WithHeader(ContentTypeHeader, AtomContentType);

        var messageBoxServiceMock = IMessageBoxService.Mock();
        messageBoxServiceMock.NoticeConfirmAsync(Any<string>(), RefStructArg<ReadOnlySpan<object>>.Any)
            .Returns(MessageBoxResult.No);

        var updateService = CreateUpdateService(messageBoxService: messageBoxServiceMock);

        using var _ = Assert.Multiple();

        await Assert.That(await updateService.CheckForUpdatesAsync()).IsFalse();
        _logger.VerifyLog()
            .ContainingMessage($"User choose to skip this version: {HigherPrereleaseVersion}");
        await Assert.That(Config.SkipVersion).IsEqualTo(SemVersion.Parse(HigherPrereleaseVersion));
    }
}