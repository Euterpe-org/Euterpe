using Euterpe.Contracts.Account;

namespace Euterpe.Core;

internal sealed partial class AuthService : IAuthService
{
    private const string AuthorizePageUrl = "https://euterpe-org.com/auth/app?redirect_uri=euterpe://auth/callback";

    private readonly AsyncExclusiveLock _lock = new();
    public AsyncManualResetEvent Ready { get; } = new(false);

    public async Task LoginAsync() => await Launcher.OpenUriAsync(AuthorizePageUrl).ConfigureAwait(false);

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

    public async Task CompleteLoginAsync(string code)
    {
        await _lock.AcquireAsync().ConfigureAwait(false);
        try
        {
            var response = await AuthClient.ExchangeAppTokenAsync(new AppTokenRequest(code)).ConfigureAwait(false);
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

    [UsedImplicitly]
    public required AuthState AuthState { get; init; }

    [UsedImplicitly]
    public required IPlatformLauncher Launcher { get; init; }

    [UsedImplicitly]
    public required IPlatformSecureStorage SecureStorage { get; init; }

    [UsedImplicitly]
    public required IEuterpeAccountClient AccountClient { get; init; }

    [UsedImplicitly]
    public required IEuterpeAuthClient AuthClient { get; init; }

    [UsedImplicitly]
    public required ILogger<AuthService> Logger { get; init; }

    #endregion Injections
}