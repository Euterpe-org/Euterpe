using MuseDashModToolsUI.Models;
using RichardSzalay.MockHttp;

namespace MuseDashModToolsUI.Test;

public class GithubServiceTest
{
    private const string LatestReleaseInfoLink = "https://api.github.com/repos/MDModsDev/MuseDashModToolsUI/releases/latest";
    private const string ReleaseInfoLink = "https://api.github.com/repos/MDModsDev/MuseDashModToolsUI/releases";

    private const string LatestReleaseInfo = """
                                             {
                                              "tag_name": "v1.10.0-rc1",
                                              "name": "Test",
                                              "body": "Test"
                                             }
                                             """;

    private const string ReleaseInfo = """
                                       [
                                        {
                                         "tag_name": "v1.10.0-rc1",
                                         "name": "Test",
                                         "body": "Test"
                                        }
                                       ]
                                       """;

    private readonly MockHttpMessageHandler _client;

    private readonly TestLogger _logger;

    public GithubServiceTest(ITestOutputHelper testOutputHelper)
    {
        _client = new MockHttpMessageHandler();
        _logger = new TestLogger(testOutputHelper);
    }

    [Fact]
    public async Task VersionTest()
    {
        _client.When(LatestReleaseInfoLink).Respond("application/json", LatestReleaseInfo);
        _client.When(ReleaseInfoLink).Respond("application/json", ReleaseInfo);
        var mockSavingService = new Mock<ISavingService>();
        var mockSettings = new Setting();
        mockSavingService.Setup(x => x.Settings).Returns(mockSettings);
        var githubService = new GitHubService
        {
            Logger = _logger,
            Client = _client.ToHttpClient(),
            MessageBoxService = new Mock<IMessageBoxService>().Object,
            SavingService = new Lazy<ISavingService>(() => mockSavingService.Object)
        };
        await githubService.CheckUpdates();
    }
}