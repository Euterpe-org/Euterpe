using System.Net;
using Euterpe.Contracts.Account;
using Euterpe.Core.Http.Clients;

namespace Euterpe.Tests;

public sealed partial class AuthServiceTest
{
    [Test]
    public async Task RestoreSessionAsync_WhenNoStoredTokens_ShouldReturnFalse()
    {
        var secureStorageMock = IPlatformSecureStorage.Mock();
        secureStorageMock.LoadTokensAsync()
            .Returns((TokenPayload?)null);
        var sut = CreateAuthService(secureStorage: secureStorageMock);

        var result = await sut.RestoreSessionAsync();

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task RestoreSessionAsync_WhenGetCurrentUserFails_ShouldReturnFalseAndClearState()
    {
        var secureStorageMock = IPlatformSecureStorage.Mock();
        secureStorageMock.LoadTokensAsync()
            .Returns(new TokenPayload(ValidAccessToken, ValidRefreshToken));
        var accountClientMock = IEuterpeAccountClient.Mock();
        accountClientMock.GetCurrentUserAsync(Any<CancellationToken>())
            .Throws(CreateApiException(HttpStatusCode.Unauthorized));
        var sut = CreateAuthService(secureStorage: secureStorageMock, accountClient: accountClientMock);

        var result = await sut.RestoreSessionAsync();

        using var _ = Assert.Multiple();
        await Assert.That(result).IsFalse();
        await Assert.That(sut.AuthState.AccessToken).IsNull();
        await Assert.That(sut.AuthState.RefreshToken).IsNull();
        await Assert.That(sut.Ready.IsSet).IsFalse();
    }

    [Test]
    public async Task RestoreSessionAsync_WhenSuccessful_ShouldSetReadyAndReturnTrue()
    {
        var secureStorageMock = IPlatformSecureStorage.Mock();
        secureStorageMock.LoadTokensAsync()
            .Returns(new TokenPayload(ValidAccessToken, ValidRefreshToken));
        var accountClientMock = IEuterpeAccountClient.Mock();
        accountClientMock.GetCurrentUserAsync(Any<CancellationToken>())
            .Returns(new CurrentUserResponse(TestUser));
        var sut = CreateAuthService(secureStorage: secureStorageMock, accountClient: accountClientMock);

        var result = await sut.RestoreSessionAsync();

        using var _ = Assert.Multiple();
        await Assert.That(result).IsTrue();
        await Assert.That(sut.AuthState.CurrentUser).IsEqualTo(TestUser);
        await Assert.That(sut.Ready.IsSet).IsTrue();
    }
}