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
        AuthState? authState = null,
        IPlatformLauncher? launcher = null,
        IPlatformSecureStorage? secureStorage = null,
        IEuterpeAccountClient? accountClient = null,
        Func<ILoopbackCallbackListener>? listenerFactory = null) =>
        new()
        {
            AuthState = authState ?? new AuthState(),
            AccountClient = accountClient ?? IEuterpeAccountClient.Mock(),
            AuthClient = authClient ?? IEuterpeAuthClient.Mock(),
            Launcher = launcher ?? IPlatformLauncher.Mock(),
            SecureStorage = secureStorage ?? IPlatformSecureStorage.Mock(),
            ListenerFactory = listenerFactory ?? (() => ILoopbackCallbackListener.Mock()),
            Logger = _logger
        };

    // Builds a service wired with a loopback that completes login successfully via LoginAsync.
    private AuthService CreateLoggableService(IEuterpeAuthClient authClient)
    {
        var (launcher, listenerFactory) = StateEchoingLoopback(AuthCode, null);
        return CreateAuthService(authClient, launcher: launcher, listenerFactory: listenerFactory);
    }

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
