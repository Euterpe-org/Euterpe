using MessagePack;

namespace Euterpe.Models.Charts;

[MessagePackObject]
public sealed class ManifestUploader
{
    [Key("uid")] public int Uid { get; set; }
    [Key("nickname")] public string Nickname { get; set; } = null!;
    [Key("charter_level")] public int CharterLevel { get; set; }
}