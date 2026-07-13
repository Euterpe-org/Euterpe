using PolyType;

namespace Euterpe.Models.Charts;

public sealed class ManifestMeta
{
    [PropertyShape(Name = "name")] public string Name { get; set; } = null!;
    [PropertyShape(Name = "name_romanized")] public string? NameRomanized { get; set; }
    [PropertyShape(Name = "author")] public string Author { get; set; } = null!;
    [PropertyShape(Name = "description")] public string? Description { get; set; }
    [PropertyShape(Name = "safe_for_streamer")] public bool SafeForStreamer { get; set; }
    [PropertyShape(Name = "bpm")] public int Bpm { get; set; }
    [PropertyShape(Name = "bpm_min")] public int? BpmMin { get; set; }
    [PropertyShape(Name = "bpm_max")] public int? BpmMax { get; set; }
    [PropertyShape(Name = "scene")] public string Scene { get; set; } = null!;
    [PropertyShape(Name = "scene_egg")] public string? SceneEgg { get; set; }
    [PropertyShape(Name = "background_video_opacity")] public float? BackgroundVideoOpacity { get; set; }
    [PropertyShape(Name = "search_keywords")] public string[]? SearchKeywords { get; set; }
    [PropertyShape(Name = "maps")] public Dictionary<string, ManifestMap> Maps { get; set; } = null!;
    [PropertyShape(Name = "hide_mode")] public string? HideMode { get; set; }
    [PropertyShape(Name = "hide_rating_override")] public string? HideRatingOverride { get; set; }
    [PropertyShape(Name = "hide_message")] public string? HideMessage { get; set; }
    [PropertyShape(Name = "cover_dominant_color")] public string? CoverDominantColor { get; set; }
    [PropertyShape(Name = "uploader")] public ManifestUploader? Uploader { get; set; }
    [PropertyShape(Name = "created_at")] public long? CreatedAt { get; set; }
    [PropertyShape(Name = "updated_at")] public long? UpdatedAt { get; set; }
}
