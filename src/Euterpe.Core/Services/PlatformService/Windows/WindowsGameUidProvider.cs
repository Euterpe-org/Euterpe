using System.Text;
using Euterpe.Contracts.Account;
using Microsoft.Win32;

namespace Euterpe.Core;

[SupportedOSPlatform(nameof(OSPlatform.Windows))]
internal sealed class WindowsGameUidProvider : IGameUidProvider
{
    private const string MuseDashRegistrySubKey = @"Software\PeroPeroGames\MuseDash";

    public async Task<MuseDashUidRequest?> GetMuseDashUidRequestAsync()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(MuseDashRegistrySubKey, false);
            var value = key?.GetValue(GameConfig.UidRegistryValueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames);

            if (value is not byte[] bytes || bytes is [])
            {
                Logger.ZLogWarning($"MuseDash user info registry value is missing or invalid");
                return null;
            }

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
            Logger.ZLogError(ex, $"Failed to get MuseDash user ID from registry");
            return null;
        }
    }

    #region Injections

    public required GameConfig GameConfig { get; init; }
    public required ILogger<WindowsGameUidProvider> Logger { get; init; }

    #endregion Injections
}
