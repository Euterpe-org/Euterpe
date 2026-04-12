using System.Net;
using Euterpe.Contracts.Account;
using Euterpe.Core.Http.Clients;

namespace Euterpe.Tests;

public sealed partial class AuthServiceTest
{
    [Test]
    public async Task RestoreSessionAsync_WhenNoStoredTokens_ShouldReturnFalse()
    {
        var platformServiceMock = IPlatformService.Mock();
        platformServiceMock.LoadTokensAsync()
            .Returns((TokenPayload?)null);
        var sut = CreateAuthService(platformService: platformServiceMock);

        var result = await sut.RestoreSessionAsync();

        await Assert.That(result).IsFalse();
    }

    [Test]
    [Timeout(5_000)]
    public async Task RestoreSessionAsync_WhenRefreshFails_ShouldReturnFalseAndClearState(CancellationToken cancellationToken)
    {
        // Issue #4: Bad tokens should be cleared on restore failure.
        // Also tests that RestoreSessionAsync doesn't hang when refresh token is rejected,
        // since RenewAccessTokenAsync triggers re-login and awaits Ready.
        var platformServiceMock = IPlatformService.Mock();
        platformServiceMock.LoadTokensAsync()
            .Returns(new TokenPayload(ValidAccessToken, ValidRefreshToken));
        var authClientMock = IEuterpeAuthClient.Mock();
        authClientMock.RefreshTokenAsync(Any<RefreshRequest>(), Any<CancellationToken>())
            .Throws(CreateApiException(HttpStatusCode.Unauthorized));
        var sut = CreateAuthService(authClientMock, platformServiceMock);

        var result = await sut.RestoreSessionAsync();

        using var _ = Assert.Multiple();
        await Assert.That(result).IsFalse();
        await Assert.That(sut.AuthState.AccessToken).IsNull();
        await Assert.That(sut.AuthState.RefreshToken).IsNull();
    }

    [Test]
    public async Task RestoreSessionAsync_WhenSuccessful_ShouldSetReadyAndReturnTrue()
    {
        var platformServiceMock = IPlatformService.Mock();
        platformServiceMock.LoadTokensAsync()
            .Returns(new TokenPayload(ValidAccessToken, ValidRefreshToken));
        var authClientMock = IEuterpeAuthClient.Mock();
        authClientMock.RefreshTokenAsync(Any<RefreshRequest>(), Any<CancellationToken>())
            .Returns(new RefreshResponse(NewAccessToken, NewRefreshToken));
        authClientMock.GetCurrentUserAsync(Any<string>(), Any<CancellationToken>())
            .Returns(TestUser);
        var sut = CreateAuthService(authClientMock, platformServiceMock);

        var result = await sut.RestoreSessionAsync();

        using var _ = Assert.Multiple();
        await Assert.That(result).IsTrue();
        await Assert.That(sut.AuthState.IsLoggedIn).IsTrue();
        await Assert.That(sut.Ready.IsSet).IsTrue();
    }
}