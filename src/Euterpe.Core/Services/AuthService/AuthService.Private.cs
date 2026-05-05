using Euterpe.Contracts.Account;

namespace Euterpe.Core;

internal sealed partial class AuthService
{
    private async Task UpdateSessionAsync(string accessToken, string refreshToken, UserInfo? currentUser)
    {
        AuthState.AccessToken = accessToken;
        AuthState.RefreshToken = refreshToken;
        AuthState.AccessTokenExpiry = DateTimeOffset.Now.Add(AccessTokenLifetime);
        AuthState.CurrentUser = currentUser;

        await SecureStorage.SaveTokensAsync(accessToken, refreshToken).ConfigureAwait(false);
    }

    private async Task ClearSessionAsync()
    {
        AuthState.Clear();
        await SecureStorage.ClearTokensAsync().ConfigureAwait(false);
        Ready.Reset();
    }
}