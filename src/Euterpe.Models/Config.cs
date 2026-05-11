using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Semver;

namespace Euterpe.Models;

public sealed partial class Config : ObservableObject
{
    // File Management Settings
    [AllowNull]
    [ObservableProperty]
    public partial string SteamFolder { get; set; } = string.Empty;

    [AllowNull]
    [ObservableProperty]
    public partial string SteamExecPath { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CacheFolder { get; set; } = Path.Combine(AppDataFolder, "Cache");

    // Per Game Settings
    [ObservableProperty]
    public partial GameId ActiveGame { get; set; } = GameId.MuseDash;

    public MuseDashConfig MuseDash { get; init; } = new();

    public MuseDash2Config MuseDash2 { get; init; } = new();

    [JsonIgnore]
    public IReadOnlyList<GameConfig> Games => field ??= [MuseDash, MuseDash2];

    [JsonIgnore]
    public GameConfig ActiveGameConfig => ActiveGame switch
    {
        GameId.MuseDash => MuseDash,
        GameId.MuseDash2 => MuseDash2,
        _ => throw new UnreachableException()
    };

    // Appearance Settings
    [AllowNull]
    public string LanguageCode { get; set; } = CultureInfo.CurrentUICulture.ToString();

    public string Theme { get; set; } = "Dark";

    // Experience Settings
    [ObservableProperty]
    public partial bool ShowConsole { get; set; } = true;

    [ObservableProperty]
    public partial bool ShowStartScreen { get; set; } = true;

    [ObservableProperty]
    public partial bool AlwaysShowScrollBar { get; set; } = true;

    // Download Settings
    [ObservableProperty]
    public partial UpdateChannel UpdateChannel { get; set; } = UpdateChannel.Stable;

    public SemVersion? SkipVersion { get; set; }

    // Advanced Settings
    [ObservableProperty]
    public partial bool IgnoreException { get; set; }
}