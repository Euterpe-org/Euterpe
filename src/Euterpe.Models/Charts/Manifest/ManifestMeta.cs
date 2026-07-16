namespace Euterpe.Models.Charts;

public sealed class ManifestMeta
{
    public string Name { get; set; } = null!;
    public string? NameRomanized { get; set; }
    public string Author { get; set; } = null!;
    public string? Description { get; set; }
    public bool SafeForStreamer { get; set; }
    public int Bpm { get; set; }
    public int? BpmMin { get; set; }
    public int? BpmMax { get; set; }
    public string Scene { get; set; } = null!;
    public string? SceneEgg { get; set; }
    public float? BackgroundVideoOpacity { get; set; }
    public string[]? SearchKeywords { get; set; }
    public Dictionary<string, ManifestMap> Maps { get; set; } = null!;
    public string? HideMode { get; set; }
    public string? HideRatingOverride { get; set; }
    public string? HideMessage { get; set; }
    public string? CoverDominantColor { get; set; }
    public ManifestUploader? Uploader { get; set; }
    public long? CreatedAt { get; set; }
    public long? UpdatedAt { get; set; }
}
