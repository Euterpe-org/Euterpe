using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Euterpe.Contracts.Account;
using static Euterpe.Core.JsonContexts.SnakeCaseJsonContext;

namespace Euterpe.Core;

internal sealed partial class WindowsService
{
    private static readonly string TokenFilePath = Path.Combine(AppDataFolder, "auth.dat");

    public async Task SaveTokensAsync(string accessToken, string refreshToken)
    {
        var payload = new TokenPayload(accessToken, refreshToken);
        var json = JsonSerializer.Serialize(payload, Default.TokenPayload);
        var plainBytes = Encoding.UTF8.GetBytes(json);
        var encrypted = ProtectedData.Protect(plainBytes, DataProtectionScope.CurrentUser);

        await File.WriteAllBytesAsync(TokenFilePath, encrypted).ConfigureAwait(false);
    }

    public async Task<TokenPayload?> LoadTokensAsync()
    {
        if (!File.Exists(TokenFilePath))
        {
            return null;
        }

        try
        {
            var encrypted = await File.ReadAllBytesAsync(TokenFilePath).ConfigureAwait(false);
            var plainBytes = ProtectedData.Unprotect(encrypted, DataProtectionScope.CurrentUser);
            var json = Encoding.UTF8.GetString(plainBytes);
            var payload = JsonSerializer.Deserialize(json, Default.TokenPayload);

            if (payload.AccessToken.IsNullOrEmpty() || payload.RefreshToken.IsNullOrEmpty())
            {
                return null;
            }

            return payload;
        }
        catch (CryptographicException ex)
        {
            Logger.ZLogWarning(ex, $"Failed to decrypt auth tokens, clearing stored data");
            await ClearTokensAsync().ConfigureAwait(false);
            return null;
        }
    }

    public async Task ClearTokensAsync()
    {
        if (!File.Exists(TokenFilePath))
        {
            return;
        }

        try
        {
            File.Delete(TokenFilePath);
        }
        catch (Exception ex)
        {
            Logger.ZLogWarning(ex, $"Failed to delete auth token file");
        }
    }
}