using Microsoft.Win32;

namespace Euterpe.Core;

[SupportedOSPlatform(nameof(OSPlatform.Windows))]
internal sealed class WindowsSteamPathDiscovery : ISteamPathDiscovery
{
    public bool TryGetSteamFolder([NotNullWhen(true)] out string? steamFolder)
    {
        if (TryGetSteamFolderFromRegistry(out steamFolder))
        {
            Logger.ZLogInformation($"Detected Steam folder from Registry: {steamFolder}");
            return true;
        }

        Logger.ZLogWarning($"Failed to get Steam folder from Registry");

        steamFolder = WindowsPaths.SteamSearch.FirstOrDefault(Directory.Exists);
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

    #region Injections

    public required Config Config { get; init; }
    public required ILogger<WindowsSteamPathDiscovery> Logger { get; init; }

    #endregion Injections
}
