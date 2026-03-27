using Avalonia.Platform.Storage;
using CliWrap;
using CliWrap.Buffered;

namespace Euterpe.Core;

[SupportedOSPlatform(nameof(OSPlatform.Linux))]
internal sealed partial class LinuxService : IPlatformService
{
    private const string DeepLinkDesktopFileName = "com.euterpe-org.Euterpe.desktop";
    private const string DotNetInstallScriptUrl = "https://dot.net/v1/dotnet-install.sh";

    private static readonly string[] LinuxPaths = new[]
        {
            ".local/share/Steam",
            ".steam/steam",
            ".var/app/ocm.valvesoftware.Steam/data/Steam",
            ".steam/steam",
            ".steam/root"
        }
        .Select(path => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), path)).ToArray();

    public string OsString => "linux";
    public string UpdaterFileName => "Updater";

    public async Task SetupDeepLinkAsync(string processPath)
    {
        try
        {
            var applicationsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "applications");
            var desktopFilePath = Path.Combine(applicationsPath, DeepLinkDesktopFileName);
            Directory.CreateDirectory(applicationsPath);

            var content =
                $"""
                 [Desktop Entry]
                 Type=Application
                 Name=Euterpe
                 Exec="{processPath.EscapeDesktopExecArgument()}" %u
                 MimeType=x-scheme-handler/{IPlatformService.DeepLinkScheme};
                 Terminal=false
                 """;

            await File.WriteAllTextAsync(desktopFilePath, content).ConfigureAwait(false);

            var result = await Cli.Wrap("xdg-mime")
                .WithArguments(["default", DeepLinkDesktopFileName, $"x-scheme-handler/{IPlatformService.DeepLinkScheme}"])
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync()
                .ConfigureAwait(false);

            if (result.ExitCode is not 0)
            {
                Logger.ZLogWarning($"xdg-mime exited with code {result.ExitCode}: {result.StandardError}");
                return;
            }

            Logger.ZLogInformation($"Registered deep link protocol on Linux with process path: {processPath}");
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to register deep link protocol on Linux");
        }
    }

    public bool TryGetSteamFolder([NotNullWhen(true)] out string? steamFolder)
    {
        steamFolder = LinuxPaths.FirstOrDefault(Directory.Exists);
        if (steamFolder is not null && CheckIsValidSteamFolder(steamFolder))
        {
            Logger.ZLogInformation($"Auto detected steam folder on Linux: {steamFolder}");
            return true;
        }

        Logger.ZLogWarning($"Auto detect steam install on common path failed.");
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
        const string relativePath = @"steamapps/common/Muse Dash";

        if (GamePathService.TryGetGameFolderFromVdf(GameConstants.MuseDashSteamAppId, relativePath, out gameFolder))
        {
            return true;
        }

        Logger.ZLogInformation($"Could not get game folder from libraryfolders.vdf");

        if (GamePathService.TryGetGameFolderFromCommonPaths(LinuxPaths, relativePath, out gameFolder))
        {
            return true;
        }

        Logger.ZLogWarning($"Failed to auto detect game path on Linux");
        return false;
    }

    public bool CheckIsValidGameFolder(string folderPath)
    {
        var exePath = Path.Combine(folderPath, "MuseDash.exe");
        var dllPath = Path.Combine(folderPath, "GameAssembly.dll");

        if (!File.Exists(exePath) || !File.Exists(dllPath))
        {
            Logger.ZLogError($"MuseDash.exe or GameAssembly.dll not found in {folderPath}");
            return false;
        }

        Logger.ZLogInformation($"MuseDash.exe and GameAssembly.dll found in {folderPath}");
        return true;
    }

    public async Task<string?> GetSteamExecPathAsync()
    {
        try
        {
            var result = await Cli.Wrap("which")
                .WithArguments("steam")
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync()
                .ConfigureAwait(false);

            if (result.ExitCode is not 0)
            {
                return null;
            }

            var path = result.StandardOutput.Trim();
            if (path.IsNullOrEmpty() || !File.Exists(path))
            {
                return null;
            }

            Logger.ZLogInformation($"Found steam executable via 'which': {path}");
            return path;
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to run 'which steam'");
            return null;
        }
    }

    public bool CheckIsValidSteamExecPath(string filePath)
    {
        try
        {
            var mode = File.GetUnixFileMode(filePath);
            const UnixFileMode executePermissions = UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute;
            var isExecutable = (mode & executePermissions) is not UnixFileMode.None;

            return isExecutable;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> CheckDotNetRuntimeInstalledAsync()
    {
        const string relativePath = $"steamapps/compatdata/{GameConstants.MuseDashSteamAppId}/pfx/drive_c/Program Files/dotnet/shared/Microsoft.WindowsDesktop.App";
        var runtimeRoot = Path.Combine(Config.SteamFolder, relativePath);

        if (!Directory.Exists(runtimeRoot))
        {
            Logger.ZLogInformation($".NET Desktop Runtime root path not found: {runtimeRoot}");
            return false;
        }

        var installed = Directory.EnumerateDirectories(runtimeRoot, "6.*", SearchOption.TopDirectoryOnly).Any();

        if (!installed)
        {
            Logger.ZLogInformation($".NET Desktop Runtime 6 not found in {runtimeRoot}");
            return false;
        }

        Logger.ZLogInformation($".NET Desktop Runtime 6 found in {runtimeRoot}");
        return true;
    }

    public async Task<bool> InstallDotNetRuntimeAsync()
    {
        if (!await CheckProtontricksInstalledAsync().ConfigureAwait(true))
        {
            await MessageBoxService.ErrorOverlayAsync(MessageBox_Content_Protontricks_Not_Installed).ConfigureAwait(false);
            return false;
        }

        if (!await ConfigureWinePrefixAsync().ConfigureAwait(true))
        {
            await MessageBoxService.ErrorOverlayAsync(MessageBox_Content_Protontricks_Wineprefix_Failed).ConfigureAwait(false);
            return false;
        }

        try
        {
            var result = await Cli.Wrap("protontricks")
                .WithArguments([GameConstants.MuseDashSteamAppId, "dotnetdesktop6"])
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync()
                .ConfigureAwait(false);

            if (result.ExitCode is 0)
            {
                Logger.ZLogInformation($".NET Runtime installed successfully via protontricks");
                return true;
            }

            Logger.ZLogError($".NET Runtime installation failed with exit code: {result.ExitCode}");
            return false;
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

    public bool CheckPathEnvironmentVariableSet()
    {
        var envValue = Environment.GetEnvironmentVariable("MD_DIRECTORY");
        return !envValue.IsNullOrEmpty() && envValue == Config.MuseDashFolder;
    }

    public bool SetPathEnvironmentVariable()
    {
        Logger.ZLogInformation($"Ask user to set MD_DIRECTORY environment variable to: {Config.MuseDashFolder}");
        MessageBoxService.NoticeConfirmOverlayAsync(MessageBox_Content_SetPathEnvironment_Linux, Config.MuseDashFolder)
            .ConfigureAwait(false);
        return true;
    }

    public void RevealFile(string filePath)
    {
        Process.Start(
            new ProcessStartInfo("xdg-open", filePath)
            {
                UseShellExecute = false,
                CreateNoWindow = true
            }
        );

        Logger.ZLogInformation($"Reveal file: {filePath}");
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
    public required IFileSystemService FileSystemService { get; init; }

    [UsedImplicitly]
    public required ILogger<LinuxService> Logger { get; init; }

    [UsedImplicitly]
    public required IGamePathService GamePathService { get; init; }

    [UsedImplicitly]
    public required IMessageBoxService MessageBoxService { get; init; }

    #endregion Injections
}