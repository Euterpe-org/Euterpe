using Euterpe.Contracts.Account;

namespace Euterpe.Abstractions;

[PlatformService(ServiceRegistrationLifetime.AppSingleton)]
public interface IPlatformSecureStorage
{
    /// <summary>
    ///     Save tokens to secure storage.
    /// </summary>
    Task SaveTokensAsync(string accessToken, string refreshToken);

    /// <summary>
    ///     Load tokens from secure storage.
    /// </summary>
    /// <returns>Token payload, or null if not found.</returns>
    Task<TokenPayload?> LoadTokensAsync();

    /// <summary>
    ///     Clear all stored tokens.
    /// </summary>
    Task ClearTokensAsync();
}
