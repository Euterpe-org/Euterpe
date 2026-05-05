using CliWrap;
using CliWrap.Buffered;

namespace Euterpe.Core;

[SupportedOSPlatform(nameof(OSPlatform.Windows))]
internal sealed class WindowsGameRuntimeInstaller : IGameRuntimeInstaller
{
    private const string DotnetRuntimeUrl = "https://aka.ms/dotnet/6.0/dotnet-runtime-win-x64.exe";

    public async Task<bool> CheckInstalledAsync()
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

    public async Task<bool> InstallAsync()
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

    #region Injections

    [UsedImplicitly]
    public required IDownloadManager DownloadManager { get; init; }

    [UsedImplicitly]
    public required ILogger<WindowsGameRuntimeInstaller> Logger { get; init; }

    #endregion Injections
}