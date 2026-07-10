using PolyType;

namespace Euterpe.Models.Sharing;

[GenerateShape]
public sealed partial class GameSharePackage
{
    public const int CurrentSchemaVersion = 1;
    public const int MaximumChartCount = 100;

    [PropertyShape(Name = "schema")]
    public int SchemaVersion { get; set; }

    [PropertyShape(Name = "game")]
    public GameId GameId { get; set; }

    [PropertyShape(Name = "cids")]
    public int[] ChartIds { get; set; } = [];

    public GameShareMod[] Mods { get; set; } = [];
}
