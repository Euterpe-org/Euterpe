using Euterpe.Contracts.Account;

namespace Euterpe.Core;

[SupportedOSPlatform(nameof(OSPlatform.OSX))]
internal sealed class MacOsSecureStorage : IPlatformSecureStorage
{
    public Task SaveTokensAsync(string accessToken, string refreshToken) => throw new NotSupportedException();
    public Task<TokenPayload?> LoadTokensAsync() => throw new NotSupportedException();
    public Task ClearTokensAsync() => throw new NotSupportedException();
}