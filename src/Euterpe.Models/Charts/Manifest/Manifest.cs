using MessagePack;

namespace Euterpe.Models.Charts;

[MessagePackObject]
public sealed class Manifest
{
    public const int CurrentSchema = 1;

    [Key("schema")] public int Schema { get; set; }
    [Key("cid")] public int? Cid { get; set; }
    [Key("meta")] public ManifestMeta Meta { get; set; } = null!;
    [Key("files")] public Dictionary<string, ManifestFileEntry> Files { get; set; } = null!;
}