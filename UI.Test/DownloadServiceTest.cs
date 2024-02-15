using MuseDashModToolsUI.Models;
using RichardSzalay.MockHttp;

namespace MuseDashModToolsUI.Test;

public sealed class DownloadServiceTest(ITestOutputHelper testOutputHelper)
{
    private const string ReleaseInfoLink = "https://api.github.com/repos/MDModsDev/MuseDashModToolsUI/releases";

    private const string ReleaseInfo = """
                                       [
                                        {
                                         "tag_name": "v1.10.0-rc1",
                                         "name": "Test",
                                         "body": "Test"
                                        }
                                       ]
                                       """;

    private readonly MockHttpMessageHandler _client = new();

    private readonly TestLogger _logger = new(testOutputHelper);

    [Fact]
    public async Task VersionTest()
    {
        _client.When(ReleaseInfoLink).Respond("application/json", ReleaseInfo);
        var mockSavingService = new Mock<ISavingService>();
        var mockSettings = new Setting();
        mockSavingService.Setup(x => x.Settings).Returns(mockSettings);
        var githubService = new DownloadService
        {
            Logger = _logger,
            Client = _client.ToHttpClient(),
            MessageBoxService = new Mock<IMessageBoxService>().Object,
            SavingService = new Lazy<ISavingService>(() => mockSavingService.Object)
        };
        await githubService.CheckUpdatesAsync();
    }
}