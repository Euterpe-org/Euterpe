using System.Net;
using Euterpe.Contracts.Account;
using Euterpe.Core.Http.Clients;
using Refit;

namespace Euterpe.Tests.Core;

public sealed partial class AuthServiceTest
{
    [Test]
    public async Task RenewAccessTokenAsync_WhenRefreshSucceeds_ShouldReturnNewTokenAndRotateRefreshToken()
    {
        var authState = CreateLoggedInState();
        var authClientMock = IEuterpeAuthClient.Mock();
        authClientMock.RefreshTokenAsync(Any<RefreshRequest>(), Any<CancellationToken>())
            .Returns(new RefreshResponse(NewAccessToken, NewRefreshToken));
        var sut = CreateAuthService(authClientMock, authState);

        var token = await sut.RenewAccessTokenAsync(ValidAccessToken);

        using var _ = Assert.Multiple();
        await Assert.That(token).IsEqualTo(NewAccessToken);
        await Assert.That(authState.RefreshToken).IsEqualTo(NewRefreshToken);
    }

    [Test]
    public async Task RenewAccessTokenAsync_WhenAnotherCallerAlreadyRefreshed_ShouldReturnCurrentTokenWithoutCallingServer()
    {
        var authState = CreateLoggedInState();
        authState.AccessToken = NewAccessToken;
        var authClientMock = IEuterpeAuthClient.Mock();
        var sut = CreateAuthService(authClientMock, authState);

        var token = await sut.RenewAccessTokenAsync(ValidAccessToken);

        using var _ = Assert.Multiple();
        await Assert.That(token).IsEqualTo(NewAccessToken);
        authClientMock.RefreshTokenAsync(Any<RefreshRequest>(), Any<CancellationToken>()).WasNeverCalled();
    }

    [Test]
    [Arguments(HttpStatusCode.Unauthorized)]
    [Arguments(HttpStatusCode.Forbidden)]
    [Arguments(HttpStatusCode.InternalServerError)]
    [Arguments(HttpStatusCode.BadGateway)]
    public async Task RenewAccessTokenAsync_WhenRefreshFails_ShouldPropagateException(HttpStatusCode statusCode)
    {
        var authState = CreateLoggedInState();
        var authClientMock = IEuterpeAuthClient.Mock();
        authClientMock.RefreshTokenAsync(Any<RefreshRequest>(), Any<CancellationToken>())
            .Throws(CreateApiException(statusCode));
        var sut = CreateAuthService(authClientMock, authState);

        Func<Task<string?>> act = async () => await sut.RenewAccessTokenAsync(ValidAccessToken);

        await Assert.That(act).ThrowsExactly<ApiException>();
    }

    private static ApiException CreateApiException(HttpStatusCode statusCode)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "https://test.com");
        var response = new HttpResponseMessage(statusCode);
        return ApiException.Create(request, HttpMethod.Post, response, new RefitSettings()).Result;
    }
}
