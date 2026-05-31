using System.Security.Cryptography;
using Euterpe.Contracts.Account;

namespace Euterpe.Core;

internal sealed partial class AuthService : IAuthService
{
    private const string ClientId = "euterpe-app";
    private const string AuthorizePageUrl = "https://euterpe-org.com/auth/app";
    private static readonly TimeSpan CallbackTimeout = TimeSpan.FromMinutes(5);

    private readonly AsyncExclusiveLock _lock = new();
    public AsyncManualResetEvent Ready { get; } = new(false);

    public async Task LoginAsync()
    {
        var pkce = PkcePair.Generate();
        var state = RandomNumberGenerator.GetBytes(32).ToBase64Url();

        using var listener = ListenerFactory();
        var redirectUri = $"http://127.0.0.1:{listener.Port}/callback";

        await Launcher.OpenUriAsync(BuildAuthorizeUrl(redirectUri, pkce.Challenge, state)).ConfigureAwait(false);

        using var cts = new CancellationTokenSource(CallbackTimeout);
        LoopbackCallbackResult callback;
        try
        {
            callback = await listener.WaitForCallbackAsync(cts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            Logger.ZLogWarning($"Login timed out waiting for the authorization callback");
            return;
        }

        if (callback.State != state)
        {
            Logger.ZLogWarning($"Login rejected: state mismatch");
            return;
        }

        if (!callback.Error.IsNullOrEmpty())
        {
            Logger.ZLogWarning($"Login failed with error: {callback.Error}");
            return;
        }

        if (callback.Code.IsNullOrEmpty())
        {
            Logger.ZLogWarning($"Login callback missing authorization code");
            return;
        }

        await ExchangeCodeAsync(callback.Code, pkce.Verifier, redirectUri).ConfigureAwait(false);
    }

    private static string BuildAuthorizeUrl(string redirectUri, string codeChallenge, string state) =>
        $"{AuthorizePageUrl}"
        + $"?client_id={ClientId}"
        + $"&redirect_uri={Uri.EscapeDataString(redirectUri)}"
        + $"&code_challenge={codeChallenge}"
        + "&code_challenge_method=S256"
        + $"&state={state}";

    public async Task LogoutAsync()
    {
        await _lock.StealAsync("logout").ConfigureAwait(false);
        try
        {
            if (AuthState.RefreshToken is not null)
            {
                try
                {
                    await AuthClient.LogoutAsync(new LogoutRequest(AuthState.RefreshToken)).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Logger.ZLogWarning(ex, $"Failed to call logout API");
                }
            }

            await ClearSessionAsync().ConfigureAwait(false);
            Logger.ZLogInformation($"User logged out");
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task ExchangeCodeAsync(string code, string codeVerifier, string redirectUri)
    {
        await _lock.AcquireAsync().ConfigureAwait(false);
        try
        {
            var response = await AuthClient.ExchangeAppTokenAsync(new AppTokenRequest(ClientId, code, codeVerifier, redirectUri)).ConfigureAwait(false);
            await UpdateSessionAsync(response.AccessToken, response.RefreshToken, response.Me).ConfigureAwait(false);

            Logger.ZLogInformation($"User logged in: {response.Me.Nickname}");

            Ready.Set();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<string> GetAccessTokenAsync()
    {
        await Ready.WaitAsync().ConfigureAwait(false);

        await _lock.AcquireAsync().ConfigureAwait(false);
        try
        {
            if (DateTimeOffset.Now < AuthState.AccessTokenExpiry)
            {
                return AuthState.AccessToken!;
            }

            var response = await AuthClient.RefreshTokenAsync(new RefreshRequest(AuthState.RefreshToken!)).ConfigureAwait(false);
            await UpdateSessionAsync(response.AccessToken, response.RefreshToken, AuthState.CurrentUser).ConfigureAwait(false);
            return AuthState.AccessToken!;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<string> RenewAccessTokenAsync(string staleToken)
    {
        await _lock.AcquireAsync().ConfigureAwait(false);
        try
        {
            if (AuthState.AccessToken != staleToken)
            {
                return AuthState.AccessToken!;
            }

            var response = await AuthClient.RefreshTokenAsync(new RefreshRequest(AuthState.RefreshToken!)).ConfigureAwait(false);
            await UpdateSessionAsync(response.AccessToken, response.RefreshToken, AuthState.CurrentUser).ConfigureAwait(false);
            return AuthState.AccessToken!;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> RestoreSessionAsync()
    {
        var tokens = await SecureStorage.LoadTokensAsync().ConfigureAwait(false);
        if (tokens is null)
        {
            return false;
        }

        AuthState.AccessToken = tokens.AccessToken;
        AuthState.RefreshToken = tokens.RefreshToken;

        Ready.Set();

        try
        {
            var response = await AccountClient.GetCurrentUserAsync().ConfigureAwait(false);
            AuthState.CurrentUser = response.User;
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to restore session");
            await ClearSessionAsync().ConfigureAwait(false);
            return false;
        }

        return true;
    }

    #region Injections

    public required AuthState AuthState { get; init; }
    public required Func<ILoopbackCallbackListener> ListenerFactory { get; init; }
    public required IPlatformLauncher Launcher { get; init; }
    public required IPlatformSecureStorage SecureStorage { get; init; }
    public required IEuterpeAccountClient AccountClient { get; init; }
    public required IEuterpeAuthClient AuthClient { get; init; }
    public required ILogger<AuthService> Logger { get; init; }

    #endregion Injections
}