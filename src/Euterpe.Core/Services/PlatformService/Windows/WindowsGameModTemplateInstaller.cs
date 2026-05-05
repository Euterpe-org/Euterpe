using CliWrap;

namespace Euterpe.Core;

[SupportedOSPlatform(nameof(OSPlatform.Windows))]
internal sealed class WindowsGameModTemplateInstaller : IGameModTemplateInstaller
{
    #region Injections

    [UsedImplicitly]
    public required ILogger<WindowsGameModTemplateInstaller> Logger { get; init; }

    #endregion Injections

    public async Task<bool> CheckInstalledAsync()
    {
        try
        {
            var result = await Cli.Wrap("dotnet")
                .WithArguments(["new", "list", "musedashmod"])
                .WithValidation(CommandResultValidation.None)
                .ExecuteAsync()
                .ConfigureAwait(false);

            return result.IsSuccess;
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to check Mod Template installation");
            return false;
        }
    }

    public async Task InstallAsync()
    {
        try
        {
            await Cli.Wrap("dotnet")
                .WithArguments(["new", "install", "MuseDash.Mod.Template"])
                .ExecuteAsync()
                .ConfigureAwait(false);

            Logger.ZLogInformation($"Mod Template installed successfully");
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to install Mod Template");
            throw;
        }
    }

    public async Task UninstallAsync()
    {
        try
        {
            await Cli.Wrap("dotnet")
                .WithArguments(["new", "uninstall", "MuseDash.Mod.Template"])
                .ExecuteAsync()
                .ConfigureAwait(false);

            Logger.ZLogInformation($"Mod Template uninstalled successfully");
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to uninstall Mod Template");
            throw;
        }
    }
}