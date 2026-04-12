using Euterpe.Contracts.Account;
using Euterpe.Core.Http.Clients;

namespace Euterpe.Tests;

public sealed partial class AuthServiceTest
{
    [Test]
    public async Task GetAccessTokenAsync_WhenTokenNotExpired_ShouldReturnCachedToken()
    {
        var authState = CreateLoggedInState();
        var sut = CreateAuthService(authState: authState);

        var token = await sut.GetAccessTokenAsync();

        await Assert.That(token).IsEqualTo(ValidAccessToken);
    }

    [Test]
    public async Task GetAccessTokenAsync_WhenTokenExpired_ShouldRenewAndReturnNewToken()
    {
        var authState = CreateExpiredState();
        var authClientMock = IEuterpeAuthClient.Mock();
        authClientMock.RefreshTokenAsync(Any<RefreshRequest>(), Any<CancellationToken>())
            .Returns(new RefreshResponse(NewAccessToken, NewRefreshToken));
        var sut = CreateAuthService(authClientMock, authState: authState);

        var token = await sut.GetAccessTokenAsync();

        await Assert.That(token).IsEqualTo(NewAccessToken);
    }
}