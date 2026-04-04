using System.Net;
using Euterpe.Contracts.Account;
using Refit;

namespace Euterpe.Core;

internal sealed partial class AuthService
{
    private bool HasRefreshableSession() => AuthState.AccessToken is not null && AuthState.RefreshToken is not null;

    private bool HasValidAccessToken() => HasRefreshableSession() && DateTime.UtcNow < AuthState.AccessTokenExpiry;

    private async Task<string?> RefreshAccessTokenCoreAsync(bool ignoreExpiry)
    {
        if (!HasRefreshableSession())
        {
            return null;
        }

        await _lock.AcquireAsync().ConfigureAwait(false);
        try
        {
            if (!ignoreExpiry && HasValidAccessToken())
            {
                return AuthState.AccessToken;
            }

            var response = await AuthClient.RefreshTokenAsync(new RefreshRequest(AuthState.RefreshToken!)).ConfigureAwait(false);
            AuthState.AccessToken = response.AccessToken;
            AuthState.AccessTokenExpiry = DateTime.UtcNow.Add(AccessTokenLifetime);

            await PlatformService.SaveTokensAsync(AuthState.AccessToken, AuthState.RefreshToken!).ConfigureAwait(false);

            return AuthState.AccessToken;
        }
        catch (ApiException ex) when (ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            Logger.ZLogError(ex, $"Refresh token rejected by server, logging out");
            await ClearSessionAsync().ConfigureAwait(false);
            return null;
        }
        catch (Exception ex)
        {
            Logger.ZLogWarning(ex, $"Failed to refresh access token due to transient error");
            return null;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task ClearSessionAsync()
    {
        AuthState.Clear();
        await PlatformService.ClearTokensAsync().ConfigureAwait(false);
        Ready.Reset();
    }
}