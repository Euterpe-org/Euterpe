using System.Net;
using System.Net.Http;
using Euterpe.Core.Http.Clients;

namespace Euterpe.Tests.Core;

public sealed partial class AuthServiceTest
{
    [Test]
    public async Task IsServerHealthyAsync_WhenStatus200_ReturnsTrue()
    {
        var healthClientMock = IEuterpeHealthClient.Mock();
        healthClientMock.CheckAsync(Any<CancellationToken>())
            .Returns(new HttpResponseMessage(HttpStatusCode.OK));
        var sut = CreateAuthService(healthClient: healthClientMock);

        var result = await sut.IsServerHealthyAsync();

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task IsServerHealthyAsync_WhenStatusNot200_ReturnsFalse()
    {
        var healthClientMock = IEuterpeHealthClient.Mock();
        healthClientMock.CheckAsync(Any<CancellationToken>())
            .Returns(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        var sut = CreateAuthService(healthClient: healthClientMock);

        var result = await sut.IsServerHealthyAsync();

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task IsServerHealthyAsync_WhenRequestThrows_ReturnsFalse()
    {
        var healthClientMock = IEuterpeHealthClient.Mock();
        healthClientMock.CheckAsync(Any<CancellationToken>())
            .Throws(new HttpRequestException("connection refused"));
        var sut = CreateAuthService(healthClient: healthClientMock);

        var result = await sut.IsServerHealthyAsync();

        await Assert.That(result).IsFalse();
    }
}
