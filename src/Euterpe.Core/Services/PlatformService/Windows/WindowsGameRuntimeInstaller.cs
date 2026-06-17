using CliWrap;
using CliWrap.Buffered;

namespace Euterpe.Core;

[SupportedOSPlatform(nameof(OSPlatform.Windows))]
internal sealed class WindowsGameRuntimeInstaller : IGameRuntimeInstaller
{
    public async Task<bool> CheckInstalledAsync()
        => CheckGameLocalRuntimeInstalled() || await CheckSystemRuntimeInstalledAsync().ConfigureAwait(false);

    public async Task InstallAsync()
    {
        var tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            Logger.ZLogInformation($"Downloading .NET Runtime from {IGameRuntimeInstaller.DotnetRuntimeUrl} to {tempFilePath}");
            await AppDownloadManager.DownloadFileAsync(IGameRuntimeInstaller.DotnetRuntimeUrl, tempFilePath).ConfigureAwait(false);

            Logger.ZLogInformation($"Extracting .NET Runtime to {GameConfig.DotnetRuntimeFolder}");
            await ArchiveService.ExtractZipFileAsync(tempFilePath, GameConfig.DotnetRuntimeFolder).ConfigureAwait(false);

            Logger.ZLogInformation($".NET Runtime installed to {GameConfig.DotnetRuntimeFolder}");
        }
        finally
        {
            FileSystemService.TryDeleteFile(tempFilePath);
        }
    }

    private bool CheckGameLocalRuntimeInstalled()
    {
        ReadOnlySpan<string> dotnetPaths =
        [
            GameConfig.DotnetRuntimeFolder,
            GameConfig.MelonLoaderDotnetRuntimeFolder
        ];

        foreach (var path in dotnetPaths)
        {
            if (!Directory.Exists(path))
            {
                continue;
            }

            Logger.ZLogInformation($"Game-local .NET runtime found: {path}");
            return true;
        }

        Logger.ZLogInformation($"No game-local .NET runtime found in {GameConfig.Folder}");
        return false;
    }

    private async Task<bool> CheckSystemRuntimeInstalledAsync()
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

    #region Injections

    public required GameConfig GameConfig { get; init; }
    public required IAppDownloadManager AppDownloadManager { get; init; }
    public required IArchiveService ArchiveService { get; init; }
    public required IFileSystemService FileSystemService { get; init; }
    public required ILogger<WindowsGameRuntimeInstaller> Logger { get; init; }

    #endregion Injections
}
