using Euterpe.Contracts.Account;

namespace Euterpe.Core;

internal sealed class AuthService : IAuthService
{
    private const string AuthorizePageUrl = "https://euterpe-org.com/auth/app?redirect_uri=euterpe://auth/callback";

    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    public AsyncGate Ready { get; } = new();

    public async Task LoginAsync() => await PlatformService.OpenUriAsync(AuthorizePageUrl).ConfigureAwait(false);

    public async Task LogoutAsync()
    {
        if (AuthState.RefreshToken is not null)
        {
            try
            {
                await ApiClient.LogoutAsync(new LogoutRequest(AuthState.RefreshToken)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.ZLogWarning(ex, $"Failed to call logout API, clearing local tokens anyway");
            }
        }

        AuthState.Clear();

        await PlatformService.ClearTokensAsync().ConfigureAwait(false);

        Logger.ZLogInformation($"User logged out");
    }

    public async Task HandleAuthCallbackAsync(string code)
    {
        try
        {
            var response = await ApiClient.ExchangeAppTokenAsync(new AppTokenRequest(code)).ConfigureAwait(false);

            AuthState.AccessToken = response.AccessToken;
            AuthState.RefreshToken = response.RefreshToken;
            AuthState.AccessTokenExpiry = DateTime.UtcNow.AddMinutes(14);
            AuthState.CurrentUser = response.Me;
            AuthState.IsLoggedIn = true;

            await PlatformService.SaveTokensAsync(response.AccessToken, response.RefreshToken).ConfigureAwait(false);

            Logger.ZLogInformation($"User logged in: {response.Me.Nickname}");

            Ready.Open();
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to exchange auth code for token");
            throw;
        }
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        if (AuthState.AccessToken is null || AuthState.RefreshToken is null)
        {
            return null;
        }

        if (DateTime.UtcNow < AuthState.AccessTokenExpiry)
        {
            return AuthState.AccessToken;
        }

        await _refreshLock.WaitAsync().ConfigureAwait(false);
        try
        {
            if (DateTime.UtcNow < AuthState.AccessTokenExpiry)
            {
                return AuthState.AccessToken;
            }

            var response = await ApiClient.RefreshTokenAsync(new RefreshRequest(AuthState.RefreshToken)).ConfigureAwait(false);
            AuthState.AccessToken = response.AccessToken;
            AuthState.AccessTokenExpiry = DateTime.UtcNow.AddMinutes(14);

            await PlatformService.SaveTokensAsync(AuthState.AccessToken, AuthState.RefreshToken).ConfigureAwait(false);

            return AuthState.AccessToken;
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to refresh access token, logging out");
            await LogoutAsync().ConfigureAwait(false);
            return null;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    public async Task<bool> TryRestoreSessionAsync()
    {
        var tokens = await PlatformService.LoadTokensAsync().ConfigureAwait(false);
        if (tokens is null)
        {
            return false;
        }

        AuthState.AccessToken = tokens.Value.AccessToken;
        AuthState.RefreshToken = tokens.Value.RefreshToken;
        AuthState.AccessTokenExpiry = DateTime.MinValue; // Force refresh on first use

        // Validate by refreshing the token
        var accessToken = await GetAccessTokenAsync().ConfigureAwait(false);
        if (accessToken is null)
        {
            return false;
        }

        AuthState.IsLoggedIn = true;
        Ready.Open();
        return true;
    }

    #region Injections

    [UsedImplicitly]
    public required AuthState AuthState { get; init; }

    [UsedImplicitly]
    public required IPlatformService PlatformService { get; init; }

    [UsedImplicitly]
    public required IEuterpeApiClient ApiClient { get; init; }

    [UsedImplicitly]
    public required ILogger<AuthService> Logger { get; init; }

    #endregion Injections
}