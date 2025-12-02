using CliWrap;

namespace MuseDashModTools.Core;

internal sealed partial class LinuxService
{
    private async Task<bool> CheckProtontricksExistsAsync()
    {
        try
        {
            var result = await Cli.Wrap("protontricks")
                .WithArguments("--version")
                .WithValidation(CommandResultValidation.None)
                .ExecuteAsync()
                .ConfigureAwait(false);

            if (result.ExitCode is 0)
            {
                return true;
            }

            Logger.ZLogError($"Protontricks check failed. ExitCode: {result.ExitCode}");
            return false;
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"protontricks not found");
            return false;
        }
    }

    private async Task<bool> RunProtontricksWineCfgAsync()
    {
        try
        {
            var result = await Cli.Wrap("protontricks")
                .WithArguments([MuseDashGameId, "winecfg"])
                .WithValidation(CommandResultValidation.None)
                .ExecuteAsync()
                .ConfigureAwait(false);

            if (result.ExitCode is 0)
            {
                Logger.ZLogInformation($"winecfg executed successfully");
                return true;
            }

            Logger.ZLogError($"winecfg execution failed with exit code: {result.ExitCode}");
            return false;
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to execute winecfg via protontricks");
            return false;
        }
    }
}