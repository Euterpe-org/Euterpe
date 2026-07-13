using PolyType;

namespace Euterpe.Models.Charts;

public sealed class ManifestFileEntry
{
    [PropertyShape(Name = "version")] public int Version { get; set; }
    [PropertyShape(Name = "size")] public long Size { get; set; }
}
