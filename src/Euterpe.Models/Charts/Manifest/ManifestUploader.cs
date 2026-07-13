using PolyType;

namespace Euterpe.Models.Charts;

public sealed class ManifestUploader
{
    [PropertyShape(Name = "uid")] public int Uid { get; set; }
    [PropertyShape(Name = "nickname")] public string Nickname { get; set; } = null!;
    [PropertyShape(Name = "charter_level")] public int CharterLevel { get; set; }
}
