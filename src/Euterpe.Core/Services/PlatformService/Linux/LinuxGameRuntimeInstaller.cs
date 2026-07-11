using CliWrap;
using CliWrap.Buffered;

namespace Euterpe.Core;

[SupportedOSPlatform(nameof(OSPlatform.Linux))]
internal sealed class LinuxGameRuntimeInstaller : IGameRuntimeInstaller
{
    private const string DllOverrideCommand = """
                                              wine reg add "HKCU\Software\Wine\DllOverrides" /v "version" /t "REG_SZ" /d "native,builtin" /f
                                              """;

    public Task<bool> CheckInstalledAsync() =>
        Task.FromResult(CheckGameLocalRuntimeInstalled() || CheckProtonPrefixRuntimeInstalled());

    public async Task InstallAsync()
    {
        if (!await CheckProtontricksInstalledAsync().ConfigureAwait(false))
        {
            await MessageBoxService.ErrorOverlayAsync(MessageBox_Content_Protontricks_Not_Installed).ConfigureAwait(false);
            throw new InvalidOperationException("Protontricks not installed");
        }

        if (!await ApplyVersionDllOverrideAsync().ConfigureAwait(false))
        {
            await MessageBoxService.ErrorOverlayAsync(MessageBox_Content_Protontricks_Wineprefix_Failed).ConfigureAwait(false);
            throw new InvalidOperationException("Failed to configure wineprefix");
        }

        var tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            Logger.ZLogInformation($"Downloading .NET Runtime from {GameConfig.DotNetRuntimeUrl} to {tempFilePath}");
            await AppDownloadManager.DownloadFileAsync(GameConfig.DotNetRuntimeUrl, tempFilePath).ConfigureAwait(false);

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

            Logger.ZLogInformation($"Game-local .NET {GameConfig.DotNetRuntimeMajorVersion} runtime found: {folder}");
            return true;
        }

        Logger.ZLogInformation($"No game-local .NET {GameConfig.DotNetRuntimeMajorVersion} runtime found in {GameConfig.Folder}");
        return false;
    }

    private bool ContainsRequiredRuntime(string sharedFrameworkFolder) =>
        Directory.Exists(sharedFrameworkFolder)
        && Directory.EnumerateDirectories(sharedFrameworkFolder, $"{GameConfig.DotNetRuntimeMajorVersion}.*", SearchOption.TopDirectoryOnly).Any();

    private bool CheckProtonPrefixRuntimeInstalled()
    {
        var relativePath = $"steamapps/compatdata/{GameConfig.SteamAppId}/pfx/drive_c/Program Files/dotnet/shared/Microsoft.WindowsDesktop.App";
        var runtimeRoot = Path.Combine(Config.SteamFolder, relativePath);

        if (!Directory.Exists(runtimeRoot))
        {
            Logger.ZLogInformation($".NET Desktop Runtime root path not found: {runtimeRoot}");
            return false;
        }

        var installed = Directory.EnumerateDirectories(runtimeRoot, $"{GameConfig.DotNetRuntimeMajorVersion}.*", SearchOption.TopDirectoryOnly).Any();

        if (!installed)
        {
            Logger.ZLogInformation($".NET Desktop Runtime {GameConfig.DotNetRuntimeMajorVersion} not found in {runtimeRoot}");
            return false;
        }

        Logger.ZLogInformation($".NET Desktop Runtime {GameConfig.DotNetRuntimeMajorVersion} found in {runtimeRoot}");
        return true;
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

    private async Task<bool> ApplyVersionDllOverrideAsync()
    {
        try
        {
            var result = await Cli.Wrap("protontricks")
                .WithArguments(["-c", DllOverrideCommand, GameConfig.SteamAppId])
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync()
                .ConfigureAwait(false);

            if (result.ExitCode is not 0)
            {
                Logger.ZLogError($"Failed to add version dll override: {result.StandardError}");
                return false;
            }

            Logger.ZLogInformation($"version dll override added successfully");
            return true;
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to apply version dll override via protontricks");
            return false;
        }
    }

    #region Injections

    public required Config Config { get; init; }
    public required GameConfig GameConfig { get; init; }
    public required IAppDownloadManager AppDownloadManager { get; init; }
    public required IArchiveService ArchiveService { get; init; }
    public required IFileSystemService FileSystemService { get; init; }
    public required ILogger<LinuxGameRuntimeInstaller> Logger { get; init; }
    public required IMessageBoxService MessageBoxService { get; init; }

    #endregion Injections
}
