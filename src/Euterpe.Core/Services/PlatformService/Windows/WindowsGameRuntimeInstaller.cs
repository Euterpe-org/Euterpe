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
            Logger.ZLogInformation($"Downloading .NET Runtime from {IGameRuntimeInstaller.DotNetRuntimeUrl} to {tempFilePath}");
            await AppDownloadManager.DownloadFileAsync(IGameRuntimeInstaller.DotNetRuntimeUrl, tempFilePath).ConfigureAwait(false);

            Logger.ZLogInformation($"Extracting .NET Runtime to {GameConfig.DotNetRuntimeFolder}");
            await ArchiveService.ExtractZipFileAsync(tempFilePath, GameConfig.DotNetRuntimeFolder).ConfigureAwait(false);

            Logger.ZLogInformation($".NET Runtime installed to {GameConfig.DotNetRuntimeFolder}");
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
            GameConfig.DotNetRuntimeFolder,
            GameConfig.MelonLoaderDotNetRuntimeFolder
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
