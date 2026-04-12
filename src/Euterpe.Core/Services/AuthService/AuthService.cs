using System.Net;
using Euterpe.Contracts.Account;
using Refit;

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
        if (DateTimeOffset.Now < AuthState.AccessTokenExpiry)
        {
            return AuthState.AccessToken!;
        }

        return await RenewAccessTokenAsync().ConfigureAwait(false);
    }

    public async Task<string> RenewAccessTokenAsync()
    {
        await _lock.AcquireAsync().ConfigureAwait(false);
        try
        {
            var response = await AuthClient.RefreshTokenAsync(new RefreshRequest(AuthState.RefreshToken!)).ConfigureAwait(false);
            await UpdateSessionAsync(response.AccessToken, response.RefreshToken, AuthState.CurrentUser).ConfigureAwait(false);
            return AuthState.AccessToken!;
        }
        catch (ApiException ex) when (ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            Logger.ZLogError(ex, $"Refresh token rejected by server, requiring re-login");
            await ClearSessionAsync().ConfigureAwait(false);
            await LoginAsync().ConfigureAwait(false);
            await Ready.WaitAsync().ConfigureAwait(false);
            return AuthState.AccessToken!;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> RestoreSessionAsync()
    {
        var tokens = await PlatformService.LoadTokensAsync().ConfigureAwait(false);
        if (tokens is null)
        {
            return false;
        }

        AuthState.AccessToken = tokens.AccessToken;
        AuthState.RefreshToken = tokens.RefreshToken;
        AuthState.AccessTokenExpiry = DateTimeOffset.MinValue;

        try
        {
            var accessToken = await GetAccessTokenAsync().ConfigureAwait(false);
            AuthState.CurrentUser = await AuthClient.GetCurrentUserAsync(accessToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to restore session");
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