using CliWrap;
using CliWrap.Buffered;

namespace Euterpe.Core;

internal sealed partial class LinuxService
{
    private async Task<bool> CheckProtontricksInstalledAsync()
    {
        try
        {
            var result = await Cli.Wrap("protontricks")
                .WithArguments("--version")
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync()
                .ConfigureAwait(false);

            if (result.ExitCode is 0)
            {
                Logger.ZLogInformation($"Protontricks found: {result.StandardOutput.Trim()}");
                return true;
            }

            Logger.ZLogError($"Protontricks check failed: {result.StandardError}");
            return false;
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Protontricks not found");
            return false;
        }
    }

    private async Task<bool> ConfigureWinePrefixAsync()
    {
        try
        {
            var winVersionResult = await Cli.Wrap("protontricks")
                .WithArguments([MuseDashGameId, "win10"])
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync()
                .ConfigureAwait(false);

            if (winVersionResult.ExitCode is not 0)
            {
                Logger.ZLogError($"Failed to set Windows version to Win10: {winVersionResult.StandardError}");
                return false;
            }

            Logger.ZLogInformation($"Windows version set to Windows 10");

            const string dllOverrideCommand = @"wine reg add 'HKEY_CURRENT_USER\Software\Wine\DllOverrides' /v version /t REG_SZ /d native,builtin /f";
            var dllOverrideResult = await Cli.Wrap("protontricks")
                .WithArguments(["-c", dllOverrideCommand, MuseDashGameId])
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync()
                .ConfigureAwait(false);

            if (dllOverrideResult.ExitCode is not 0)
            {
                Logger.ZLogError($"Failed to add version dll override: {dllOverrideResult.StandardError}");
                return false;
            }

            Logger.ZLogInformation($"version dll override added successfully");
            return true;
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to configure Wine prefix via protontricks");
            return false;
        }
    }
}