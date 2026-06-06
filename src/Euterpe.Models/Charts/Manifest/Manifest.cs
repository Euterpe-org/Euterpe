using MessagePack;

namespace Euterpe.Models.Charts;

[MessagePackObject]
public sealed class Manifest
{
    [Key("schema")] public int Schema { get; set; }
    [Key("cid")] public int? Cid { get; set; }
    [Key("meta")] public ManifestMeta Meta { get; set; } = null!;
    [Key("files")] public Dictionary<string, ManifestFileEntry> Files { get; set; } = null!;
}