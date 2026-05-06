using CliWrap;
using CliWrap.Buffered;

namespace Euterpe.Core;

[SupportedOSPlatform(nameof(OSPlatform.Linux))]
internal sealed class LinuxGameRuntimeInstaller : IGameRuntimeInstaller
{
    private const string DllOverrideCommand = """
                                              wine reg add "HKCU\Software\Wine\DllOverrides" /v "version" /t "REG_SZ" /d "native,builtin" /f
                                              """;

    public async Task<bool> CheckInstalledAsync()
    {
        var relativePath = $"steamapps/compatdata/{GameConfig.SteamAppId}/pfx/drive_c/Program Files/dotnet/shared/Microsoft.WindowsDesktop.App";
        var runtimeRoot = Path.Combine(Config.SteamFolder, relativePath);

        if (!Directory.Exists(runtimeRoot))
        {
            Logger.ZLogInformation($".NET Desktop Runtime root path not found: {runtimeRoot}");
            return false;
        }

        var installed = Directory.EnumerateDirectories(runtimeRoot, "6.*", SearchOption.TopDirectoryOnly).Any();

        if (!installed)
        {
            Logger.ZLogInformation($".NET Desktop Runtime 6 not found in {runtimeRoot}");
            return false;
        }

        Logger.ZLogInformation($".NET Desktop Runtime 6 found in {runtimeRoot}");
        return true;
    }

    public async Task InstallAsync()
    {
        if (!await CheckProtontricksInstalledAsync().ConfigureAwait(false))
        {
            await MessageBoxService.ErrorOverlayAsync(MessageBox_Content_Protontricks_Not_Installed).ConfigureAwait(false);
            throw new InvalidOperationException("Protontricks not installed");
        }

        if (!await ConfigureWinePrefixAsync().ConfigureAwait(false))
        {
            await MessageBoxService.ErrorOverlayAsync(MessageBox_Content_Protontricks_Wineprefix_Failed).ConfigureAwait(false);
            throw new InvalidOperationException("Failed to configure wineprefix");
        }

        var result = await Cli.Wrap("protontricks")
            .WithArguments([GameConfig.SteamAppId, "dotnetdesktop6"])
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync()
            .ConfigureAwait(false);

        if (result.ExitCode is not 0)
        {
            throw new InvalidOperationException($"protontricks dotnetdesktop6 install failed with exit code {result.ExitCode}: {result.StandardError}");
        }

        Logger.ZLogInformation($".NET Runtime installed successfully via protontricks");
    }

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
                .WithArguments([GameConfig.SteamAppId, "win10"])
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync()
                .ConfigureAwait(false);

            if (winVersionResult.ExitCode is not 0)
            {
                Logger.ZLogError($"Failed to set Windows version to Win10: {winVersionResult.StandardError}");
                return false;
            }

            Logger.ZLogInformation($"Windows version set to Windows 10");

            var dllOverrideResult = await Cli.Wrap("protontricks")
                .WithArguments(["-c", DllOverrideCommand, GameConfig.SteamAppId])
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

    #region Injections

    [UsedImplicitly]
    public required Config Config { get; init; }

    [UsedImplicitly]
    public required GameConfig GameConfig { get; init; }

    [UsedImplicitly]
    public required ILogger<LinuxGameRuntimeInstaller> Logger { get; init; }

    [UsedImplicitly]
    public required IMessageBoxService MessageBoxService { get; init; }

    #endregion Injections
}