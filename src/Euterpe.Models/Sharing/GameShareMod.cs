using PolyType;

namespace Euterpe.Models.Sharing;

public sealed class GameShareMod
{
    public string Name { get; set; } = string.Empty;

    [PropertyShape(Name = "disabled")]
    public bool IsDisabled { get; set; }
}
