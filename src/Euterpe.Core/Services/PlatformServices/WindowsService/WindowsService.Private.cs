using Microsoft.Win32;

namespace Euterpe.Core;

internal sealed partial class WindowsService
{
    private static bool TryGetSteamFolderFromRegistry(out string steamFolder)
    {
        steamFolder = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam", "InstallPath", null)
            as string ?? string.Empty;
        return Directory.Exists(steamFolder);
    }
}