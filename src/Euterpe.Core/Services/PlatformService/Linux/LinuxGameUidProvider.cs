using System.Text;
using CliWrap;
using CliWrap.Buffered;
using Euterpe.Contracts.Account;

namespace Euterpe.Core;

[SupportedOSPlatform(nameof(OSPlatform.Linux))]
internal sealed class LinuxGameUidProvider : IGameUidProvider
{
    private const string MuseDashRegistryPath = @"HKCU\Software\PeroPeroGames\MuseDash";

    private string MuseDashUserInfoCommand => $"""
                                               wine reg query "{MuseDashRegistryPath}" /v "{GameConfig.UidRegistryValueName}"
                                               """;

    public async Task<MuseDashUidRequest?> GetMuseDashUidRequestAsync()
    {
        try
        {
            var result = await Cli.Wrap("protontricks")
                .WithArguments(["-c", MuseDashUserInfoCommand, GameConfig.SteamAppId])
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync()
                .ConfigureAwait(false);

            if (result.ExitCode is not 0)
            {
                Logger.ZLogWarning($"Failed to query MuseDash user info from registry: {result.StandardError}");
                return null;
            }

            var hex = result.StandardOutput.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
            if (hex.IsNullOrEmpty())
            {
                Logger.ZLogWarning($"Failed to read registry value from wine reg output");
                return null;
            }

            var bytes = Convert.FromHexString(hex);
            var uid = Encoding.UTF8.GetString(bytes).TrimEnd('\0');
            if (uid.IsNullOrEmpty())
            {
                Logger.ZLogWarning($"MuseDash user info registry value is empty");
                return null;
            }

            Logger.ZLogInformation($"Successfully retrieved MuseDash user info from registry");
            return new MuseDashUidRequest(uid);
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to retrieve MuseDash user info");
            return null;
        }
    }

    #region Injections

    [UsedImplicitly]
    public required GameConfig GameConfig { get; init; }

    [UsedImplicitly]
    public required ILogger<LinuxGameUidProvider> Logger { get; init; }

    #endregion Injections
}