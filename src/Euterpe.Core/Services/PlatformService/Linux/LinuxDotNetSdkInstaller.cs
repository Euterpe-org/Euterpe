using CliWrap;
using CliWrap.Buffered;

namespace Euterpe.Core;

[SupportedOSPlatform(nameof(OSPlatform.Linux))]
internal sealed class LinuxDotNetSdkInstaller : IDotNetSdkInstaller
{
    private const string DotNetInstallScriptUrl = "https://dot.net/v1/dotnet-install.sh";

    public async Task<bool> CheckInstalledAsync()
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

    public async Task<bool> InstallAsync()
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

    #region Injections

    [UsedImplicitly]
    public required IDownloadManager DownloadManager { get; init; }

    [UsedImplicitly]
    public required IFileSystemService FileSystemService { get; init; }

    [UsedImplicitly]
    public required ILogger<LinuxDotNetSdkInstaller> Logger { get; init; }

    #endregion Injections
}