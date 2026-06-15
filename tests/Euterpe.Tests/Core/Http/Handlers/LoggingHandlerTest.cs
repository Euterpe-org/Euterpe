using System.Net;
using Euterpe.Core.Http.Handlers;
using Euterpe.Tests.TestSupport;
using Microsoft.Extensions.Logging;

namespace Euterpe.Tests.Core.Http.Handlers;

[Category("LoggingHandlerTests")]
[TestSubject(typeof(LoggingHandler))]
public sealed class LoggingHandlerTest
{
    [Test]
    public async Task SendAsync_SuccessResponse_DoesNotLog()
    {
        var inner = new FakeHttpMessageHandler(HttpStatusCode.OK, "ok");
        var logger = Mock.Logger<LoggingHandler>();
        using var handler = new LoggingHandler(logger) { InnerHandler = inner };
        using var client = new HttpClient(handler);

        var response = await client.GetAsync("https://example.test/");

        using var assertions = Assert.Multiple();
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(logger.Entries).IsEmpty();
    }

    [Test]
    public async Task SendAsync_NonSuccess_LogsWarningWithBody()
    {
        var inner = new FakeHttpMessageHandler(HttpStatusCode.BadRequest, "bad-payload");
        var logger = Mock.Logger<LoggingHandler>();
        using var handler = new LoggingHandler(logger) { InnerHandler = inner };
        using var client = new HttpClient(handler);

        var response = await client.GetAsync("https://example.test/api/things");

        using var assertions = Assert.Multiple();
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        await Assert.That(logger.Entries).HasSingleItem();
        var entry = logger.Entries[0];
        await Assert.That(entry.LogLevel).IsEqualTo(LogLevel.Warning);
        await Assert.That(entry.Message).Contains("400");
        await Assert.That(entry.Message).Contains("bad-payload");
        await Assert.That(entry.Message).Contains("https://example.test/api/things");
    }
}
