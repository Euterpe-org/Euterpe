using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Euterpe.Contracts.Account;
using static Euterpe.Core.JsonContexts.SnakeCaseJsonContext;

namespace Euterpe.Core;

internal sealed partial class WindowsService
{
    private static string TokenFilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Euterpe", "auth.dat");

    public async Task SaveTokensAsync(string accessToken, string refreshToken)
    {
        var payload = new TokenPayload(accessToken, refreshToken);
        var json = JsonSerializer.Serialize(payload, Default.TokenPayload);
        var plainBytes = Encoding.UTF8.GetBytes(json);
        var encrypted = ProtectedData.Protect(plainBytes, null, DataProtectionScope.CurrentUser);

        var dir = Path.GetDirectoryName(TokenFilePath)!;
        Directory.CreateDirectory(dir);
        await File.WriteAllBytesAsync(TokenFilePath, encrypted).ConfigureAwait(false);
    }

    public async Task<(string AccessToken, string RefreshToken)?> LoadTokensAsync()
    {
        if (!File.Exists(TokenFilePath))
        {
            return null;
        }

        try
        {
            var encrypted = await File.ReadAllBytesAsync(TokenFilePath).ConfigureAwait(false);
            var plainBytes = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
            var json = Encoding.UTF8.GetString(plainBytes);
            var payload = JsonSerializer.Deserialize(json, Default.TokenPayload);

            if (payload.AccessToken.IsNullOrEmpty() || payload.RefreshToken.IsNullOrEmpty())
            {
                return null;
            }

            return (payload.AccessToken, payload.RefreshToken);
        }
        catch (CryptographicException ex)
        {
            Logger.ZLogWarning(ex, $"Failed to decrypt auth tokens, clearing stored data");
            await ClearTokensAsync().ConfigureAwait(false);
            return null;
        }
    }

    public Task ClearTokensAsync()
    {
        if (File.Exists(TokenFilePath))
        {
            File.Delete(TokenFilePath);
        }

        return Task.CompletedTask;
    }
}