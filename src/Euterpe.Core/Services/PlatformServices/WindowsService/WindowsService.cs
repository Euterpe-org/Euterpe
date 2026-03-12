using Avalonia.Platform.Storage;
using CliWrap;
using CliWrap.Buffered;

namespace Euterpe.Core;

[SupportedOSPlatform(nameof(OSPlatform.Windows))]
internal sealed partial class WindowsService : IPlatformService
{
    private const string DotnetRuntimeUrl = "https://aka.ms/dotnet/6.0/dotnet-runtime-win-x64.exe";
    private const string DotnetSdkUrl = "https://aka.ms/dotnet/10.0/dotnet-sdk-win-x64.exe";

    private static readonly string[] WindowsPaths = new[]
        {
            @"Program Files\Steam",
            @"Program Files (x86)\Steam",
            @"Program Files\SteamLibrary",
            @"Program Files (x86)\SteamLibrary",
            @"Steam",
            @"SteamLibrary"
        }
        .SelectMany(path => Environment.GetLogicalDrives().Select(drive => Path.Combine(drive, path))).ToArray();

    public string OsString => "win";
    public string UpdaterFileName => "Updater.exe";

    public bool TryGetSteamFolder([NotNullWhen(true)] out string? steamFolder)
    {
        if (TryGetSteamFolderFromRegistry(out steamFolder))
        {
            Logger.ZLogInformation($"Detected Steam folder from Registry: {steamFolder}");
            return true;
        }

        Logger.ZLogWarning($"Failed to get Steam folder from Registry");

        steamFolder = WindowsPaths.FirstOrDefault(Directory.Exists);
        if (steamFolder is not null && CheckIsValidSteamFolder(steamFolder))
        {
            Logger.ZLogInformation($"Auto detected Steam folder on Windows: {steamFolder}");
            return true;
        }

        Logger.ZLogWarning($"Auto detect Steam install on common path failed.");
        return false;
    }

    public bool CheckIsValidSteamFolder(string folderPath)
    {
        var steamAppsPath = Path.Combine(folderPath, "steamapps");
        if (Directory.Exists(steamAppsPath))
        {
            Logger.ZLogInformation($"Valid Steam folder: {folderPath}");
            return true;
        }

        Logger.ZLogError($"Invalid Steam folder: {folderPath}");
        return false;
    }

    public bool TryGetGameFolder([NotNullWhen(true)] out string? gameFolder)
    {
        const string relativePath = @"steamapps\common\Muse Dash";

        if (GamePathService.TryGetGameFolderFromVdf(GameConstants.MuseDashSteamAppId, relativePath, out gameFolder))
        {
            return true;
        }

        Logger.ZLogInformation($"Could not get game folder from libraryfolders.vdf");

        if (GamePathService.TryGetGameFolderFromCommonPaths(WindowsPaths, relativePath, out gameFolder))
        {
            return true;
        }

        Logger.ZLogWarning($"Failed to auto detect game path on Windows");
        return false;
    }

    public bool CheckIsValidGameFolder(string folderPath)
    {
        var exePath = Path.Combine(folderPath, "MuseDash.exe");
        var dllPath = Path.Combine(folderPath, "GameAssembly.dll");

        if (File.Exists(exePath) && File.Exists(dllPath))
        {
            Logger.ZLogInformation($"MuseDash.exe and GameAssembly.dll found in {folderPath}");
            return true;
        }

        Logger.ZLogError($"MuseDash.exe or GameAssembly.dll not found in {folderPath}");
        return false;
    }

    public async Task<string?> GetSteamExecPathAsync()
    {
        var steamExecPath = Path.Combine(Config.SteamFolder, "steam.exe");
        if (File.Exists(steamExecPath))
        {
            Logger.ZLogInformation($"steam.exe found at: {steamExecPath}");
            return steamExecPath;
        }

        Logger.ZLogError($"steam.exe not found at: {steamExecPath}");
        return null;
    }

    public bool CheckIsValidSteamExecPath(string filePath)
    {
        try
        {
            var info = FileVersionInfo.GetVersionInfo(filePath);
            var isValve = info.CompanyName?.Contains("Valve", StringComparison.OrdinalIgnoreCase) ?? false;
            var isSteam = info.ProductName?.Contains("Steam", StringComparison.OrdinalIgnoreCase) ?? false;

            return isValve || isSteam;
        }
        catch
        {
            return false;
        }
    }

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
        try
        {
            var tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
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
                return false;
            }

            await process.WaitForExitAsync().ConfigureAwait(false);
            Logger.ZLogInformation($".NET SDK installer finished with exit code: {process.ExitCode}");

            File.Delete(tempFilePath);

            return process.ExitCode is 0;
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to install .NET SDK");
            return false;
        }
    }

    public void RevealFile(string filePath)
    {
        Process.Start(
            new ProcessStartInfo("explorer.exe", $"/select, {filePath}")
            {
                UseShellExecute = false,
                CreateNoWindow = true
            }
        );
        Logger.ZLogInformation($"Reveal file: {filePath}");
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

    public async Task OpenFolderAsync(string folderPath)
    {
        await TopLevel.Launcher.LaunchDirectoryInfoAsync(new DirectoryInfo(folderPath)).ConfigureAwait(false);
        Logger.ZLogInformation($"Open folder: {folderPath}");
    }

    public async Task OpenFileAsync(string filePath)
    {
        await TopLevel.Launcher.LaunchFileInfoAsync(new FileInfo(filePath)).ConfigureAwait(false);
        Logger.ZLogInformation($"Open file: {filePath}");
    }

    public async Task OpenUriAsync(string uri)
    {
        await TopLevel.Launcher.LaunchUriAsync(new Uri(uri)).ConfigureAwait(false);
        Logger.ZLogInformation($"Open uri: {uri}");
    }

    #region Injections

    [UsedImplicitly]

    public required Config Config { get; init; }

    [UsedImplicitly]
    public required TopLevelProxy TopLevel { get; init; }

    [UsedImplicitly]
    public required IDownloadManager DownloadManager { get; init; }

    [UsedImplicitly]
    public required ILogger<WindowsService> Logger { get; init; }

    [UsedImplicitly]
    public required IGamePathService GamePathService { get; init; }

    [UsedImplicitly]
    public required IMessageBoxService MessageBoxService { get; init; }

    #endregion Injections
}