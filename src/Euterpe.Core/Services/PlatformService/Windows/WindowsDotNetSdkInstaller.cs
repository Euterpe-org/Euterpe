using CliWrap;
using CliWrap.Buffered;

namespace Euterpe.Core;

[SupportedOSPlatform(nameof(OSPlatform.Windows))]
internal sealed class WindowsDotNetSdkInstaller : IDotNetSdkInstaller
{
    private const string DotnetSdkUrl = "https://aka.ms/dotnet/10.0/dotnet-sdk-win-x64.exe";

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

    #region Injections

    [UsedImplicitly]
    public required IDownloadManager DownloadManager { get; init; }

    [UsedImplicitly]
    public required IFileSystemService FileSystemService { get; init; }

    [UsedImplicitly]
    public required ILogger<WindowsDotNetSdkInstaller> Logger { get; init; }

    #endregion Injections
}