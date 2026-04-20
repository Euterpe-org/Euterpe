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

    [AllowNull]
    [ObservableProperty]
    public partial string MuseDashFolder { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CacheFolder { get; set; } = Path.Combine(AppDataFolder, "Cache");

    // Game Settings
    [ObservableProperty]
    public partial GameMode GameMode { get; set; } = GameMode.Modded;

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

    [ObservableProperty]
    public partial bool SetupCompleted { get; set; }

    // Download Settings
    [ObservableProperty]
    public partial UpdateChannel UpdateChannel { get; set; } = UpdateChannel.Stable;

    public SemVersion? SkipVersion { get; set; }

    // Advanced Settings
    [ObservableProperty]
    public partial bool IgnoreException { get; set; }

    // Game Information
    [JsonIgnore]
    public string GameVersion { get; set; } = string.Empty;

    [JsonIgnore]
    public string UnityVersion { get; set; } = string.Empty;

    [JsonIgnore]
    [ObservableProperty]
    public partial string? MelonLoaderVersion { get; set; }

    [JsonIgnore]
    public SemVersion? MelonLoaderSemVersion { get; private set; }

    // Ignored Paths
    private string EuterpeChartsFolder => Path.Combine(MuseDashFolder, "Euterpe_Charts");

    [JsonIgnore]
    public string OnlineChartsFolder => Path.Combine(EuterpeChartsFolder, "Online");

    [JsonIgnore]
    public string OfflineChartsFolder => Path.Combine(EuterpeChartsFolder, "Offline");

    [JsonIgnore]
    public string ModsFolder => Path.Combine(MuseDashFolder, "Mods");

    [JsonIgnore]
    public string UserDataFolder => Path.Combine(MuseDashFolder, "UserData");

    [JsonIgnore]
    public string UserLibsFolder => Path.Combine(MuseDashFolder, "UserLibs");

    [JsonIgnore]
    public string MelonLoaderFolder => Path.Combine(MuseDashFolder, "MelonLoader");

    [JsonIgnore]
    public string MelonLoaderZipPath => Path.Combine(MuseDashFolder, "MelonLoader.zip");

    [JsonIgnore]
    public string LatestLogPath => Path.Combine(MelonLoaderFolder, "Latest.log");

    [JsonIgnore]
    private string Il2CppAssemblyGeneratorFolderPath => Path.Combine(MelonLoaderFolder, "Dependencies", "Il2CppAssemblyGenerator");

    [JsonIgnore]
    public string UnityDependencyZipPath => Path.Combine(Il2CppAssemblyGeneratorFolderPath, $"UnityDependencies_{UnityVersion}.zip");

    [JsonIgnore]
    public string Cpp2ILExecutablePath => Path.Combine(Il2CppAssemblyGeneratorFolderPath, "Cpp2IL", "Cpp2IL.exe");

    [JsonIgnore]
    public string Cpp2ILPluginPath => Path.Combine(Il2CppAssemblyGeneratorFolderPath, "Cpp2IL", "Plugins", "Cpp2IL.Plugin.StrippedCodeRegSupport.dll");

    partial void OnMelonLoaderVersionChanged(string? value) =>
        MelonLoaderSemVersion = SemVersion.TryParse(value, out var version) ? version : null;
}