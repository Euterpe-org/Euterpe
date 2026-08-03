using System.Net;
using System.Text;
using Euterpe.Core.Http.Clients;
using Euterpe.Tests.TestSupport;
using Refit;
using static Euterpe.Shared.EuterpeApi;

namespace Euterpe.Tests.Core.Http.Clients;

[Category("IEuterpeLogClientTests")]
[TestSubject(typeof(IEuterpeLogClient))]
public sealed class IEuterpeLogClientTest
{
    [Test]
    public async Task UploadLogAsync_LogFile_SendsExpectedMultipartRequest()
    {
        using var http = Mock.HttpHandler();
        http.OnPost("/api/logs/upload").Respond(HttpStatusCode.NoContent);
        var api = http.CreateEuterpeClient<IEuterpeLogClient>(Logs.BasePath);
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes("critical log"));
        var file = new StreamPart(stream, "app.log.gz", "application/gzip");

        using var response = await api.UploadLogAsync(file, "app");

        var body = http.Requests[0].Body;
        using var assertions = Assert.Multiple();
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);
        await Assert.That(body).Contains("name=file");
        await Assert.That(body).Contains("filename=app.log.gz");
        await Assert.That(body).Contains("Content-Type: application/gzip");
        await Assert.That(body).Contains("name=category");
        await Assert.That(body).Contains("app");
    }
}
