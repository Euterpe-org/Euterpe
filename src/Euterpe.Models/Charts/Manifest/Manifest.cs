using PolyType;

namespace Euterpe.Models.Charts;

[GenerateShape]
public sealed partial class Manifest
{
    public const int CurrentSchema = 1;

    [PropertyShape(Name = "schema")] public int Schema { get; set; }
    [PropertyShape(Name = "cid")] public int? Cid { get; set; }
    [PropertyShape(Name = "meta")] public ManifestMeta Meta { get; set; } = null!;
    [PropertyShape(Name = "files")] public Dictionary<string, ManifestFileEntry> Files { get; set; } = null!;
}
