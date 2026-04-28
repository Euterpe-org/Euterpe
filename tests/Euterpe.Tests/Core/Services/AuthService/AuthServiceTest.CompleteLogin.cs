using Euterpe.Contracts.Account;
using Euterpe.Core.Http.Clients;

namespace Euterpe.Tests;

public sealed partial class AuthServiceTest
{
    [Test]
    public async Task CompleteLoginAsync_ShouldSetReadyAndUpdateState()
    {
        var authClientMock = IEuterpeAuthClient.Mock();
        authClientMock.ExchangeAppTokenAsync(Any<AppTokenRequest>(), Any<CancellationToken>())
            .Returns(new AppTokenResponse(ValidAccessToken, ValidRefreshToken, TestUser));
        var sut = CreateAuthService(authClientMock);

        await sut.CompleteLoginAsync(AuthCode);

        using var _ = Assert.Multiple();
        await Assert.That(sut.Ready.IsSet).IsTrue();
        await Assert.That(sut.AuthState.AccessToken).IsEqualTo(ValidAccessToken);
        await Assert.That(sut.AuthState.RefreshToken).IsEqualTo(ValidRefreshToken);
        await Assert.That(sut.AuthState.CurrentUser).IsEqualTo(TestUser);
    }

    [Test]
    public async Task CompleteLoginAsync_WhenExchangeFails_ShouldNotSetReady()
    {
        var authClientMock = IEuterpeAuthClient.Mock();
        authClientMock.ExchangeAppTokenAsync(Any<AppTokenRequest>(), Any<CancellationToken>())
            .Throws(new HttpRequestException("Network error"));
        var sut = CreateAuthService(authClientMock);

        var act = () => sut.CompleteLoginAsync(AuthCode);

        await Assert.That(act).ThrowsException();
        await Assert.That(sut.Ready.IsSet).IsFalse();
    }
}