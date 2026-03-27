using CliWrap;
using CliWrap.Buffered;

namespace Euterpe.Core;

internal sealed partial class LinuxService
{
    private const string DotNetInstallScriptUrl = "https://dot.net/v1/dotnet-install.sh";

    public async Task<bool> CheckDotNetRuntimeInstalledAsync()
    {
        const string relativePath = $"steamapps/compatdata/{GameConstants.MuseDashSteamAppId}/pfx/drive_c/Program Files/dotnet/shared/Microsoft.WindowsDesktop.App";
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

    public async Task<bool> CheckDotNetSdkInstalledAsync()
    {
        try
        {
            var result = await Cli.Wrap("dotnet")
                .WithArguments("--list-sdks")
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync()
                .ConfigureAwait(false);

            return result.IsSuccess && !result.StandardOutput.IsNullOrEmpty();
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to check .NET SDK installation");
            return false;
        }
    }

    public async Task<bool> CheckModTemplateInstalledAsync()
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

    public async Task<bool> InstallDotNetRuntimeAsync()
    {
        if (!await CheckProtontricksInstalledAsync().ConfigureAwait(true))
        {
            await MessageBoxService.ErrorOverlayAsync(MessageBox_Content_Protontricks_Not_Installed).ConfigureAwait(false);
            return false;
        }

        if (!await ConfigureWinePrefixAsync().ConfigureAwait(true))
        {
            await MessageBoxService.ErrorOverlayAsync(MessageBox_Content_Protontricks_Wineprefix_Failed).ConfigureAwait(false);
            return false;
        }

        try
        {
            var result = await Cli.Wrap("protontricks")
                .WithArguments([GameConstants.MuseDashSteamAppId, "dotnetdesktop6"])
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync()
                .ConfigureAwait(false);

            if (result.ExitCode is 0)
            {
                Logger.ZLogInformation($".NET Runtime installed successfully via protontricks");
                return true;
            }

            Logger.ZLogError($".NET Runtime installation failed with exit code: {result.ExitCode}");
            return false;
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to install .NET Runtime");
            return false;
        }
    }

    public async Task<bool> InstallDotNetSdkAsync()
    {
        var tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            await DownloadManager.DownloadFileAsync(DotNetInstallScriptUrl, tempFilePath).ConfigureAwait(false);
            Logger.ZLogInformation($"Downloaded .NET install script to {tempFilePath}");

            var chmodResult = await Cli.Wrap("chmod")
                .WithArguments(["+x", tempFilePath])
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync()
                .ConfigureAwait(false);

            if (chmodResult.ExitCode is not 0)
            {
                Logger.ZLogError($"Failed to chmod dotnet-install.sh. ExitCode: {chmodResult.ExitCode}, Error:{chmodResult.StandardError}");
                return false;
            }

            var installResult = await Cli.Wrap("bash")
                .WithArguments([tempFilePath, "--version", "latest"])
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync()
                .ConfigureAwait(false);

            if (installResult.ExitCode is not 0)
            {
                Logger.ZLogError($".NET SDK installation failed. ExitCode: {installResult.ExitCode}, StdErr: {installResult.StandardError}");
                return false;
            }

            Logger.ZLogInformation($".NET SDK installation completed successfully");
            return true;
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to install .NET SDK");
            return false;
        }
        finally
        {
            FileSystemService.TryDeleteFile(tempFilePath);
        }
    }

    public async Task InstallModTemplateAsync()
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

    public async Task UninstallModTemplateAsync()
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

    public bool CheckPathEnvironmentVariableSet()
    {
        var envValue = Environment.GetEnvironmentVariable("MD_DIRECTORY");
        return !envValue.IsNullOrEmpty() && envValue == Config.MuseDashFolder;
    }

    public bool SetPathEnvironmentVariable()
    {
        Logger.ZLogInformation($"Ask user to set MD_DIRECTORY environment variable to: {Config.MuseDashFolder}");
        MessageBoxService.NoticeConfirmOverlayAsync(MessageBox_Content_SetPathEnvironment_Linux, Config.MuseDashFolder)
            .ConfigureAwait(false);
        return true;
    }
}