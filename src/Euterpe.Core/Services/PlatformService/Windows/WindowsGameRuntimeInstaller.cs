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

    public async Task InstallAsync()
    {
        var tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            Logger.ZLogInformation($"Downloading .NET Runtime from {DotnetRuntimeUrl} to {tempFilePath}");
            await AppDownloadManager.DownloadFileAsync(DotnetRuntimeUrl, tempFilePath).ConfigureAwait(false);

            Logger.ZLogInformation($"Launching .NET Runtime installer: {tempFilePath}");
            using var process = Process.Start(
                new ProcessStartInfo(tempFilePath)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true
                });

            if (process is null)
            {
                throw new InvalidOperationException($"Failed to start .NET Runtime installer process at {tempFilePath}");
            }

            await process.WaitForExitAsync().ConfigureAwait(false);
            Logger.ZLogInformation($".NET Runtime installer finished with exit code: {process.ExitCode}");

            if (process.ExitCode is not 0)
            {
                throw new InvalidOperationException($".NET Runtime installer exited with code {process.ExitCode}");
            }
        }
        finally
        {
            FileSystemService.TryDeleteFile(tempFilePath);
        }
    }

    #region Injections

    [UsedImplicitly]
    public required IAppDownloadManager AppDownloadManager { get; init; }

    [UsedImplicitly]
    public required IFileSystemService FileSystemService { get; init; }

    [UsedImplicitly]
    public required ILogger<WindowsGameRuntimeInstaller> Logger { get; init; }

    #endregion Injections
}