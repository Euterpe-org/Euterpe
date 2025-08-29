using Microsoft.Win32;
using MuseDashModTools.Models.VDFs;
using ValveKeyValue;

namespace MuseDashModTools.Core;

internal sealed partial class WindowsService
{
    private IEnumerable<string> GetSteamLibraries()
    {
        var stream = File.OpenRead(Path.Combine(Config.SteamFolder, @"steamapps\libraryfolders.vdf"));
        var kv = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);
        var data = kv.Deserialize<Dictionary<string, LibraryFolder>>(stream);

        return data.Values.Select(x => x.Path.Replace(@"\\", @"\"));
    }


    private static bool TryGetSteamFolderFromRegistry(out string steamFolder)
    {
        steamFolder = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam", "InstallPath", null)
            as string ?? string.Empty;
        return Directory.Exists(steamFolder);
    }
}