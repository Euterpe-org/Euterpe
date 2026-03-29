namespace Euterpe.Abstractions;

public interface IPlatformSecureStorage
{
    /// <summary>
    ///     Save tokens to secure storage.
    /// </summary>
    Task SaveTokensAsync(string accessToken, string refreshToken);

    /// <summary>
    ///     Load tokens from secure storage.
    /// </summary>
    /// <returns>Access token and refresh token, or null if not found.</returns>
    Task<(string AccessToken, string RefreshToken)?> LoadTokensAsync();

    /// <summary>
    ///     Clear all stored tokens.
    /// </summary>
    Task ClearTokensAsync();
}