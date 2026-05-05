using CliWrap;

namespace Euterpe.Core;

[SupportedOSPlatform(nameof(OSPlatform.Linux))]
internal sealed class LinuxGameModTemplateInstaller : IGameModTemplateInstaller
{
    public async Task<bool> CheckInstalledAsync()
    {
        try
        {
            var result = await Cli.Wrap("dotnet")
                .WithArguments(["new", "list", GameConfig.ModTemplateShortName])
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
                .WithArguments(["new", "install", GameConfig.ModTemplatePackageName])
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
                .WithArguments(["new", "uninstall", GameConfig.ModTemplatePackageName])
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

    #region Injections

    [UsedImplicitly]
    public required GameConfig GameConfig { get; init; }

    [UsedImplicitly]
    public required ILogger<LinuxGameModTemplateInstaller> Logger { get; init; }

    #endregion Injections
}