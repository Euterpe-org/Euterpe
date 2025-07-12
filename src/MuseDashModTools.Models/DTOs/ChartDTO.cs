using Avalonia.Media.Imaging;

namespace MuseDashModTools.Models;

public sealed class ChartDTO : ObservableObject
{
    public int GetHighestLevel() =>
        Sheets.Select(x => x.Difficulty)
            .Max(x => x.ParseLevel());

    #region DTO Properties

    public Bitmap? Cover { get; set; }

    public string EasyLevel => Sheets[0].Difficulty;

    public string HardLevel => Sheets[1].Difficulty;

    public string MasterLevel => Sheets[2].Difficulty;

    public string HiddenLevel => Sheets[3].Difficulty;

    #endregion DTO Properties

    #region Chart Properties

    public Analytic Analytics { get; set; } = null!;
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? TitleRomanized { get; set; }
    public string Artist { get; set; } = string.Empty;
    public string Charter { get; set; } = string.Empty;
    public string Bpm { get; set; } = string.Empty;
    public float Length { get; set; }
    public int OwnerUid { get; set; }
    public Sheet[] Sheets { get; set; } = [];
    public bool Ranked { get; set; }
    public string[] SearchTags { get; set; } = [];
    public DateTime Timestamp { get; set; }
    public int V { get; set; }

    #endregion Chart Properties
}