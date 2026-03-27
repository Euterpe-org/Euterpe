using CliWrap;
using CliWrap.Buffered;

namespace Euterpe.Core;

internal sealed partial class WindowsService
{
    private const string DotnetRuntimeUrl = "https://aka.ms/dotnet/6.0/dotnet-runtime-win-x64.exe";
    private const string DotnetSdkUrl = "https://aka.ms/dotnet/10.0/dotnet-sdk-win-x64.exe";

    public async Task<bool> CheckDotNetRuntimeInstalledAsync()
    {
        try
        {
            var result = await Cli.Wrap("dotnet")
                .WithArguments("--list-runtimes")
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync()
                .ConfigureAwait(false);

            return result.IsSuccess && result.StandardOutput.Contains("Microsoft.NETCore.App 6.");
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to check .NET runtime installation");
            return false;
        }
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
        try
        {
            var tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Logger.ZLogInformation($"Downloading .NET Runtime from {DotnetRuntimeUrl} to {tempFilePath}");
            await DownloadManager.DownloadFileAsync(DotnetRuntimeUrl, tempFilePath).ConfigureAwait(false);

            Logger.ZLogInformation($"Launching .NET Runtime installer: {tempFilePath}");
            using var process = Process.Start(
                new ProcessStartInfo(tempFilePath)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true
                });

            if (process is null)
            {
                return false;
            }

            await process.WaitForExitAsync().ConfigureAwait(false);
            Logger.ZLogInformation($".NET Runtime installer finished with exit code: {process.ExitCode}");

            File.Delete(tempFilePath);

            return process.ExitCode is 0;
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
            Logger.ZLogInformation($"Downloading .NET SDK from {DotnetSdkUrl} to {tempFilePath}");
            await DownloadManager.DownloadFileAsync(DotnetSdkUrl, tempFilePath).ConfigureAwait(false);

            Logger.ZLogInformation($"Launching .NET SDK installer: {tempFilePath}");
            using var process = Process.Start(
                new ProcessStartInfo(tempFilePath)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true
                });

            if (process is null)
            {
                Logger.ZLogError($"Failed to launch .NET SDK installer. Process.Start returned null");
                return false;
            }

            await process.WaitForExitAsync().ConfigureAwait(false);

            if (process.ExitCode is not 0)
            {
                Logger.ZLogError($".NET SDK installer exited with code {process.ExitCode}");
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
        try
        {
            Logger.ZLogInformation($"Set MD_DIRECTORY environment variable to: {Config.MuseDashFolder}");
            Environment.SetEnvironmentVariable("MD_DIRECTORY", Config.MuseDashFolder, EnvironmentVariableTarget.User);
            MessageBoxService.SuccessOverlayAsync(MessageBox_Content_SetPathEnvironment_Windows, Config.MuseDashFolder).ConfigureAwait(false);
            return true;
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to set MD_DIRECTORY environment variable");
            return false;
        }
    }
}