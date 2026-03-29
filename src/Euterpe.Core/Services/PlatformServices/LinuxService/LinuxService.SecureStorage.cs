using System.Text.Json;
using CliWrap;
using CliWrap.Buffered;
using Euterpe.Contracts.Account;
using static Euterpe.Core.JsonContexts.SnakeCaseJsonContext;

namespace Euterpe.Core;

internal sealed partial class LinuxService
{
    private const string SecretToolLabel = "Euterpe";
    private const string SecretToolAttrApp = "euterpe";

    private static string TokenFilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Euterpe", "auth.json");

    public async Task SaveTokensAsync(string accessToken, string refreshToken)
    {
        var payload = new TokenPayload(accessToken, refreshToken);

        if (await TrySaveToSecretServiceAsync(payload).ConfigureAwait(false))
        {
            return;
        }

        await SaveToFileAsync(payload).ConfigureAwait(false);
    }

    public async Task<(string AccessToken, string RefreshToken)?> LoadTokensAsync()
    {
        var payload = await TryLoadFromSecretServiceAsync().ConfigureAwait(false)
                      ?? await LoadFromFileAsync().ConfigureAwait(false);

        if (payload is null || payload.Value.AccessToken.IsNullOrEmpty() || payload.Value.RefreshToken.IsNullOrEmpty())
        {
            return null;
        }

        return (payload.Value.AccessToken, payload.Value.RefreshToken);
    }

    public async Task ClearTokensAsync()
    {
        try
        {
            await Cli.Wrap("secret-tool")
                .WithArguments(["clear", "app", SecretToolAttrApp])
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync()
                .ConfigureAwait(false);
        }
        catch
        {
            // secret-tool not available, ignore
        }

        if (File.Exists(TokenFilePath))
        {
            File.Delete(TokenFilePath);
        }
    }

    private async Task<bool> TrySaveToSecretServiceAsync(TokenPayload payload)
    {
        try
        {
            var json = JsonSerializer.Serialize(payload, Default.TokenPayload);

            var result = await Cli.Wrap("secret-tool")
                .WithArguments(["store", "--label", SecretToolLabel, "app", SecretToolAttrApp])
                .WithStandardInputPipe(PipeSource.FromString(json))
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync()
                .ConfigureAwait(false);

            return result.ExitCode is 0;
        }
        catch
        {
            return false;
        }
    }

    private async Task<TokenPayload?> TryLoadFromSecretServiceAsync()
    {
        try
        {
            var result = await Cli.Wrap("secret-tool")
                .WithArguments(["lookup", "app", SecretToolAttrApp])
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync()
                .ConfigureAwait(false);

            if (result.ExitCode is not 0 || result.StandardOutput.IsNullOrEmpty())
            {
                return null;
            }

            return JsonSerializer.Deserialize(result.StandardOutput, Default.TokenPayload);
        }
        catch
        {
            return null;
        }
    }

    private async Task SaveToFileAsync(TokenPayload payload)
    {
        var json = JsonSerializer.Serialize(payload, Default.TokenPayload);
        var dir = Path.GetDirectoryName(TokenFilePath)!;
        Directory.CreateDirectory(dir);
        await File.WriteAllTextAsync(TokenFilePath, json).ConfigureAwait(false);
        File.SetUnixFileMode(TokenFilePath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
    }

    private async Task<TokenPayload?> LoadFromFileAsync()
    {
        if (!File.Exists(TokenFilePath))
        {
            return null;
        }

        try
        {
            var json = await File.ReadAllTextAsync(TokenFilePath).ConfigureAwait(false);
            return JsonSerializer.Deserialize(json, Default.TokenPayload);
        }
        catch
        {
            return null;
        }
    }
}