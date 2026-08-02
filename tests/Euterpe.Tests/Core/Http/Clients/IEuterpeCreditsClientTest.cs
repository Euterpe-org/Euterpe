using Euterpe.Contracts.Credits;
using Euterpe.Core.Http.Clients;
using Euterpe.Tests.TestSupport;

namespace Euterpe.Tests.Core.Http.Clients;

[Category("IEuterpeCreditsClientTests")]
[TestSubject(typeof(IEuterpeCreditsClient))]
public sealed class IEuterpeCreditsClientTest
{
    [Test]
    public async Task GetCreditsAsync_GoldenServerJson_MapsContributorFieldsAndLanguageQuery()
    {
        using var http = Mock.HttpHandler();
        http.OnGet("/api/public/credits?lang=zh-CN")
            .RespondWithJson(
                """
                {
                  "sections": [
                    {
                      "id": "app",
                      "title": "应用程序",
                      "items": [
                        {
                          "id": "maintainer",
                          "kind": "person",
                          "featured": true,
                          "name": "Maintainer",
                          "avatar": "/static/images/maintainer.webp",
                          "description": "维护项目",
                          "links": [
                            {
                              "name": "GitHub",
                              "url": "https://github.com/maintainer"
                            }
                          ]
                        }
                      ]
                    }
                  ]
                }
                """);
        var api = http.CreateEuterpeClient<IEuterpeCreditsClient>(EuterpeApi.Public.BasePath);

        var response = await api.GetCreditsAsync("zh-CN");

        var person = response.Sections[0].Items[0];
        using var assertions = Assert.Multiple();
        await Assert.That(response.Sections[0].Title).IsEqualTo("应用程序");
        await Assert.That(person.Name).IsEqualTo("Maintainer");
        await Assert.That(person.Avatar).IsEqualTo("/static/images/maintainer.webp");
        await Assert.That(person.Links[0].Name).IsEqualTo("GitHub");
        await Assert.That(http.Requests.Single().RequestUri!.AbsoluteUri)
            .IsEqualTo("https://euterpe-org.com/api/public/credits?lang=zh-CN");
    }
}
