using Microsoft.Win32;

namespace Euterpe.Core;

internal sealed partial class WindowsService
{
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

    private static bool TryGetSteamFolderFromRegistry(out string steamFolder)
    {
        steamFolder = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam", "InstallPath", null)
            as string ?? string.Empty;
        return Directory.Exists(steamFolder);
    }
}