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
            Logger.LogInformation("Downloading .NET Runtime from {RuntimeUrl} to {TempFilePath}", GameConfig.DotNetRuntimeUrl, tempFilePath);
            await AppDownloadManager.DownloadFileAsync(GameConfig.DotNetRuntimeUrl, tempFilePath).ConfigureAwait(false);

            Logger.LogInformation("Extracting .NET Runtime to {RuntimeFolder}", GameConfig.DotNetRuntimeFolder);
            await ArchiveService.ExtractZipFileAsync(tempFilePath, GameConfig.DotNetRuntimeFolder).ConfigureAwait(false);

            Logger.LogInformation(".NET Runtime installed to {RuntimeFolder}", GameConfig.DotNetRuntimeFolder);
        }
        finally
        {
            FileSystemService.TryDeleteFile(tempFilePath);
        }
    }

    private bool CheckGameLocalRuntimeInstalled()
    {
        ReadOnlySpan<string> sharedFrameworkFolders =
        [
            GameConfig.DotNetSharedFrameworkFolder,
            GameConfig.MelonLoaderDotNetSharedFrameworkFolder
        ];

        foreach (var folder in sharedFrameworkFolders)
        {
            if (!ContainsRequiredRuntime(folder))
            {
                continue;
            }

            Logger.LogInformation("Game-local .NET {RuntimeMajorVersion} runtime found: {Folder}", GameConfig.DotNetRuntimeMajorVersion, folder);
            return true;
        }

        Logger.LogInformation("No game-local .NET {RuntimeMajorVersion} runtime found in {GameFolder}", GameConfig.DotNetRuntimeMajorVersion, GameConfig.Folder);
        return false;
    }

    private bool ContainsRequiredRuntime(string sharedFrameworkFolder) =>
        Directory.Exists(sharedFrameworkFolder)
        && Directory.EnumerateDirectories(sharedFrameworkFolder, $"{GameConfig.DotNetRuntimeMajorVersion}.*", SearchOption.TopDirectoryOnly).Any();

    private async Task<bool> CheckSystemRuntimeInstalledAsync()
    {
        try
        {
            var result = await Cli.Wrap("dotnet")
                .WithArguments("--list-runtimes")
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync()
                .ConfigureAwait(false);

            return result.IsSuccess && result.StandardOutput.Contains($"Microsoft.NETCore.App {GameConfig.DotNetRuntimeMajorVersion}.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to check .NET runtime installation");
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
