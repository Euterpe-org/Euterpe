namespace MuseDashModTools.Tests.UpdateServiceTests;

[Category("UpdateServiceTests")]
[TestSubject(typeof(UpdateService))]
public sealed class UpdateServiceGeneralTests : UpdateServiceTestBase
{
    [Test]
    public async Task CheckForUpdatesAsync_WithInvalidUpdateSource_ThrowsUnreachableException()
    {
        var updateService = new UpdateService
        {
            Config = new Config
            {
                UpdateSource = (UpdateSource)10
            },
            Client = Mock.Of<HttpClient>(),
            Logger = Logger,
            DownloadManager = Mock.Of<IDownloadManager>(),
            MessageBoxService = Mock.Of<IMessageBoxService>(),
            PlatformService = Mock.Of<IPlatformService>()
        };

        await Assert.ThrowsAsync<UnreachableException>(() => updateService.CheckForUpdatesAsync());
    }
}