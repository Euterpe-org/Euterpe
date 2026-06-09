using MessagePack;

namespace Euterpe.Models.Charts;

[MessagePackObject]
public sealed class ManifestFileEntry
{
    [Key("version")] public int Version { get; set; }
    [Key("size")] public long Size { get; set; }
}
