using Microsoft.Win32;
using MuseDashModTools.Models.VDFs;
using ValveKeyValue;

namespace MuseDashModTools.Core;

internal sealed partial class WindowsService
{
    private IEnumerable<string> GetCandidateFolders() =>
        TryGetSteamLibraries(out var libraryFolders) ? libraryFolders : WindowsPaths;

    private bool TryGetSteamLibraries(out IEnumerable<string> libraryFolders)
    {
        libraryFolders = [];

        var vdfPath = Path.Combine(Config.SteamFolder, @"steamapps\libraryfolders.vdf");
        if (!File.Exists(vdfPath))
        {
            Logger.ZLogWarning($"Steam libraryfolders.vdf not found at {vdfPath}");
            return false;
        }

        try
        {
            var stream = File.OpenRead(vdfPath);
            var kv = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);
            var data = kv.Deserialize<Dictionary<string, LibraryFolder>>(stream);

            libraryFolders = data.Values.Select(x => x.Path.Replace(@"\\", @"\"));
            return true;
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Deserialize libraryfolders.vdf failed");
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