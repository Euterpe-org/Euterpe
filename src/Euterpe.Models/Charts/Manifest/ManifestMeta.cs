using MessagePack;

namespace Euterpe.Models.Charts;

[MessagePackObject]
public sealed class ManifestMeta
{
    [Key("name")] public string Name { get; set; } = null!;
    [Key("name_romanized")] public string? NameRomanized { get; set; }
    [Key("author")] public string Author { get; set; } = null!;
    [Key("description")] public string? Description { get; set; }
    [Key("safe_for_streamer")] public bool SafeForStreamer { get; set; }
    [Key("bpm")] public int Bpm { get; set; }
    [Key("bpm_min")] public int? BpmMin { get; set; }
    [Key("bpm_max")] public int? BpmMax { get; set; }
    [Key("scene")] public string Scene { get; set; } = null!;
    [Key("scene_egg")] public string? SceneEgg { get; set; }
    [Key("background_video_opacity")] public double? BackgroundVideoOpacity { get; set; }
    [Key("search_keywords")] public string[]? SearchKeywords { get; set; }
    [Key("maps")] public Dictionary<string, ManifestMap> Maps { get; set; } = null!;
    [Key("hide_mode")] public string? HideMode { get; set; }
    [Key("hide_rating_override")] public string? HideRatingOverride { get; set; }
    [Key("hide_message")] public string? HideMessage { get; set; }
    [Key("cover_dominant_color")] public string? CoverDominantColor { get; set; }
    [Key("uploader")] public ManifestUploader? Uploader { get; set; }
    [Key("created_at")] public long? CreatedAt { get; set; }
    [Key("updated_at")] public long? UpdatedAt { get; set; }
}