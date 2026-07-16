using PolyType;

namespace Euterpe.Models.Charts;

[GenerateShape]
public sealed partial class Manifest
{
    public const int CurrentSchema = 1;

    public int Schema { get; set; }
    public int? Cid { get; set; }
    public ManifestMeta Meta { get; set; } = null!;
    public Dictionary<string, ManifestFileEntry> Files { get; set; } = null!;
}
