using System.Net;
using Euterpe.Contracts.Account;
using Euterpe.Core.Http.Clients;

namespace Euterpe.Tests;

public sealed partial class AuthServiceTest
{
    [Test]
    public async Task RenewAccessTokenAsync_WhenRefreshSucceeds_ShouldReturnNewToken()
    {
        var authState = CreateLoggedInState();
        var authClientMock = IEuterpeAuthClient.Mock();
        authClientMock.RefreshTokenAsync(Any<RefreshRequest>(), Any<CancellationToken>())
            .Returns(new RefreshResponse(NewAccessToken, NewRefreshToken));
        var sut = CreateAuthService(authClientMock, authState: authState);

        var token = await sut.RenewAccessTokenAsync();

        await Assert.That(token).IsEqualTo(NewAccessToken);
        await Assert.That(authState.RefreshToken).IsEqualTo(NewRefreshToken);
    }

    [Test]
    [Timeout(5_000)]
    public async Task RenewAccessTokenAsync_WhenRefreshTokenRejected_ShouldNotDeadlock(CancellationToken cancellationToken)
    {
        // Issue #1: Deadlock detection.
        // RenewAccessTokenAsync holds _lock, gets 401, calls ClearSession + LoginAsync,
        // then awaits Ready. But CompleteLoginAsync also needs _lock.
        // If the lock isn't released before awaiting Ready, this test will timeout.
        var authState = CreateLoggedInState();
        var authClientMock = IEuterpeAuthClient.Mock();
        authClientMock.RefreshTokenAsync(Any<RefreshRequest>(), Any<CancellationToken>())
            .Throws(CreateApiException(HttpStatusCode.Unauthorized));
        authClientMock.ExchangeAppTokenAsync(Any<AppTokenRequest>(), Any<CancellationToken>())
            .Returns(new AppTokenResponse(NewAccessToken, NewRefreshToken, TestUser));
        var sut = CreateAuthService(authClientMock, authState: authState);

        var renewTask = sut.RenewAccessTokenAsync();

        // Simulate browser callback after a short delay
        await Task.Delay(200, cancellationToken);
        await sut.CompleteLoginAsync(AuthCode);

        // If there's a deadlock, the test will timeout
        var token = await renewTask;

        await Assert.That(token).IsEqualTo(NewAccessToken);
    }

    [Test]
    [Timeout(5_000)]
    public async Task RenewAccessTokenAsync_WhenRefreshTokenRejected_ConcurrentCallersShouldAllComplete(CancellationToken cancellationToken)
    {
        // Multiple concurrent callers should all eventually get a token after re-login
        var authState = CreateLoggedInState();
        var authClientMock = IEuterpeAuthClient.Mock();
        authClientMock.RefreshTokenAsync(Any<RefreshRequest>(), Any<CancellationToken>())
            .Throws(CreateApiException(HttpStatusCode.Unauthorized));
        authClientMock.ExchangeAppTokenAsync(Any<AppTokenRequest>(), Any<CancellationToken>())
            .Returns(new AppTokenResponse(NewAccessToken, NewRefreshToken, TestUser));
        var sut = CreateAuthService(authClientMock, authState: authState);

        var renewTask1 = sut.RenewAccessTokenAsync();
        var renewTask2 = sut.RenewAccessTokenAsync();

        await Task.Delay(200, cancellationToken);
        await sut.CompleteLoginAsync(AuthCode);

        var results = await Task.WhenAll(renewTask1, renewTask2);

        await Assert.That(results[0]).IsEqualTo(NewAccessToken);
        await Assert.That(results[1]).IsEqualTo(NewAccessToken);
    }
}