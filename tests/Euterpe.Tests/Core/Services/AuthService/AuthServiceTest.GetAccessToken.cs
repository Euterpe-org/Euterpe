using Euterpe.Contracts.Account;
using Euterpe.Core.Http.Clients;

namespace Euterpe.Tests.Core;

public sealed partial class AuthServiceTest
{
    [Test]
    public async Task GetAccessTokenAsync_WhenTokenNotExpired_ShouldReturnCachedToken()
    {
        var authState = CreateLoggedInState();
        var sut = CreateAuthService(authState: authState);
        sut.Ready.Set();

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
        var sut = CreateAuthService(authClientMock, authState);
        sut.Ready.Set();

        var token = await sut.GetAccessTokenAsync();

        await Assert.That(token).IsEqualTo(NewAccessToken);
    }

    [Test]
    [Timeout(5_000)]
    public async Task GetAccessTokenAsync_WhenNotLoggedIn_ShouldBlockUntilReady(CancellationToken cancellationToken)
    {
        var authClientMock = IEuterpeAuthClient.Mock();
        authClientMock.ExchangeAppTokenAsync(Any<AppTokenRequest>(), Any<CancellationToken>())
            .Returns(new AppTokenResponse(NewAccessToken, NewRefreshToken, TestUser));
        var sut = CreateLoginReadyService(authClientMock);

        var getTokenTask = sut.GetAccessTokenAsync();

        await Assert.That(getTokenTask.IsCompleted).IsFalse();

        await Task.Delay(200, cancellationToken);
        await sut.LoginAsync();

        var token = await getTokenTask;

        await Assert.That(token).IsEqualTo(NewAccessToken);
    }
}
