using Euterpe.Contracts.Account;

namespace Euterpe.Core;

internal sealed partial class AuthService : IAuthService
{
    private const string AuthorizePageUrl = "https://euterpe-org.com/auth/app?redirect_uri=euterpe://auth/callback";

    // 15 minutes but use 14 to be safe and account for clock skew and network delays
    private static readonly TimeSpan AccessTokenLifetime = TimeSpan.FromMinutes(14);

    private readonly AsyncExclusiveLock _lock = new();
    public AsyncManualResetEvent Ready { get; } = new(false);

    public async Task LoginAsync() => await PlatformService.OpenUriAsync(AuthorizePageUrl).ConfigureAwait(false);

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
                    Logger.ZLogWarning(ex, $"Failed to call logout API, clearing local tokens anyway");
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

    public async Task HandleAuthCallbackAsync(string code)
    {
        await _lock.AcquireAsync().ConfigureAwait(false);
        try
        {
            var response = await AuthClient.ExchangeAppTokenAsync(new AppTokenRequest(code)).ConfigureAwait(false);

            AuthState.AccessToken = response.AccessToken;
            AuthState.RefreshToken = response.RefreshToken;
            AuthState.AccessTokenExpiry = DateTime.UtcNow.Add(AccessTokenLifetime);
            AuthState.CurrentUser = response.Me;
            AuthState.IsLoggedIn = true;

            await PlatformService.SaveTokensAsync(response.AccessToken, response.RefreshToken).ConfigureAwait(false);

            Logger.ZLogInformation($"User logged in: {response.Me.Nickname}");

            Ready.Set();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        if (!HasRefreshableSession())
        {
            return null;
        }

        if (HasValidAccessToken())
        {
            return AuthState.AccessToken;
        }

        return await RefreshAccessTokenCoreAsync(false).ConfigureAwait(false);
    }

    public async Task<string?> RefreshAccessTokenAsync() => await RefreshAccessTokenCoreAsync(true).ConfigureAwait(false);

    public async Task<bool> TryRestoreSessionAsync()
    {
        var tokens = await PlatformService.LoadTokensAsync().ConfigureAwait(false);
        if (tokens is null)
        {
            return false;
        }

        AuthState.AccessToken = tokens.AccessToken;
        AuthState.RefreshToken = tokens.RefreshToken;
        AuthState.AccessTokenExpiry = DateTime.MinValue;

        var accessToken = await GetAccessTokenAsync().ConfigureAwait(false);
        if (accessToken is null)
        {
            return false;
        }

        try
        {
            AuthState.CurrentUser = await AuthClient.GetMeAsync(accessToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to fetch user info after session restore");
            return false;
        }

        AuthState.IsLoggedIn = true;
        Ready.Set();
        return true;
    }

    #region Injections

    [UsedImplicitly]
    public required AuthState AuthState { get; init; }

    [UsedImplicitly]
    public required IPlatformService PlatformService { get; init; }

    [UsedImplicitly]
    public required IEuterpeAuthClient AuthClient { get; init; }

    [UsedImplicitly]
    public required ILogger<AuthService> Logger { get; init; }

    #endregion Injections
}