using Avalonia.Platform.Storage;
using CliWrap;
using CliWrap.Buffered;

namespace MuseDashModTools.Core;

[SupportedOSPlatform(nameof(OSPlatform.Linux))]
internal sealed partial class LinuxService : IPlatformService
{
    private static readonly string[] LinuxPaths = new[]
        {
            ".local/share/Steam",
            ".steam/steam",
            ".var/app/ocm.valvesoftware.Steam/data/Steam",
            ".steam/steam",
            ".steam/root"
        }
        .Select(path => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), path)).ToArray();

    public string OsString => "Linux";
    public string UpdaterFileName => "Updater";

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

        if (GamePathService.TryGetGameFolderFromVdf(MuseDashGameId, relativePath, out gameFolder))
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

    public Task<bool> InstallDotNetRuntimeAsync() => throw new NotSupportedException();

    public Task<bool> InstallDotNetSdkAsync() => throw new NotSupportedException();

    public bool SetPathEnvironmentVariable() => throw new NotSupportedException();

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
    public required ILogger<LinuxService> Logger { get; init; }

    [UsedImplicitly]
    public required IGamePathService GamePathService { get; init; }

    #endregion Injections
}