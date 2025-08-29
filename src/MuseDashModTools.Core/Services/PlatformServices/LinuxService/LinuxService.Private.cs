using MuseDashModTools.Models.VDFs;
using ValveKeyValue;

namespace MuseDashModTools.Core;

internal sealed partial class LinuxService
{
    private IEnumerable<string> GetSteamLibraries()
    {
        var stream = File.OpenRead(Path.Combine(Config.SteamFolder, @"steamapps/libraryfolders.vdf"));
        var kv = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);
        var data = kv.Deserialize<Dictionary<string, LibraryFolder>>(stream);

        return data.Values.Select(x => x.Path);
    }
}