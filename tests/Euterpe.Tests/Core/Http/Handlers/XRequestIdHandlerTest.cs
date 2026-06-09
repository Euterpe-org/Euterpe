using Euterpe.Core.Http.Handlers;
using Euterpe.Tests.TestSupport;

namespace Euterpe.Tests;

[Category("XRequestIdHandlerTests")]
[TestSubject(typeof(XRequestIdHandler))]
public sealed class XRequestIdHandlerTest
{
    [Test]
    public async Task SendAsync_AddsXRequestIdHeader()
    {
        var inner = new FakeHttpMessageHandler();
        using var handler = new XRequestIdHandler { InnerHandler = inner };
        using var client = new HttpClient(handler);

        await client.GetAsync("https://example.test/");

        var capturedRequest = inner.Requests.Single();
        var values = capturedRequest.Headers.GetValues("X-Request-Id").ToArray();
        using var assertions = Assert.Multiple();
        await Assert.That(values).HasSingleItem();
        await Assert.That(Guid.TryParse(values[0], out _)).IsTrue();
    }

    [Test]
    public async Task SendAsync_GeneratesDifferentIdPerRequest()
    {
        var inner = new FakeHttpMessageHandler();
        using var handler = new XRequestIdHandler { InnerHandler = inner };
        using var client = new HttpClient(handler);

        await client.GetAsync("https://example.test/a");
        await client.GetAsync("https://example.test/b");

        var firstId = inner.Requests[0].Headers.GetValues("X-Request-Id").Single();
        var secondId = inner.Requests[1].Headers.GetValues("X-Request-Id").Single();
        await Assert.That(firstId).IsNotEqualTo(secondId);
    }
}
