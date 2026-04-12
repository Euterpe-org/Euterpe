using Euterpe.Contracts.Account;
using Euterpe.Core.Http.Clients;
using TUnit.Mocks.Logging;

namespace Euterpe.Tests;

public sealed partial class AuthServiceTest
{
    private const string ValidAccessToken = "valid-access-token";
    private const string ValidRefreshToken = "valid-refresh-token";
    private const string NewAccessToken = "new-access-token";
    private const string NewRefreshToken = "new-refresh-token";
    private const string AuthCode = "auth-code";
    private static readonly UserInfo TestUser = new(1, 0, "test@test.com", "TestUser", null, false, false, false);

    private readonly MockLogger<AuthService> _logger = Mock.Logger<AuthService>();

    private AuthService CreateAuthService(
        IEuterpeAuthClient? authClient = null,
        IPlatformService? platformService = null,
        AuthState? authState = null) =>
        new()
        {
            AuthState = authState ?? new AuthState(),
            AuthClient = authClient ?? IEuterpeAuthClient.Mock(),
            PlatformService = platformService ?? IPlatformService.Mock(),
            Logger = _logger
        };

    private static AuthState CreateLoggedInState() => new()
    {
        AccessToken = ValidAccessToken,
        RefreshToken = ValidRefreshToken,
        AccessTokenExpiry = DateTimeOffset.Now.AddMinutes(14)
    };

    private static AuthState CreateExpiredState() => new()
    {
        AccessToken = ValidAccessToken,
        RefreshToken = ValidRefreshToken,
        AccessTokenExpiry = DateTimeOffset.Now.AddMinutes(-1)
    };
}