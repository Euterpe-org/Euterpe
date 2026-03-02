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
    public partial string CacheFolder { get; set; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), nameof(Euterpe), "Cache");

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

    // Ignored Paths
    [JsonIgnore]
    public string ChartFolder => GetCombinedPath(CacheFolder, "Charts");

    [JsonIgnore]
    public string CustomAlbumsFolder => GetCombinedPath(MuseDashFolder, "Custom_Albums");

    [JsonIgnore]
    public string ModsFolder => GetCombinedPath(MuseDashFolder, "Mods");

    [JsonIgnore]
    public string UserDataFolder => GetCombinedPath(MuseDashFolder, "UserData");

    [JsonIgnore]
    public string UserLibsFolder => GetCombinedPath(MuseDashFolder, "UserLibs");

    [JsonIgnore]
    public string MelonLoaderFolder => GetCombinedPath(MuseDashFolder, "MelonLoader");

    [JsonIgnore]
    public string MelonLoaderZipPath => GetCombinedPath(MuseDashFolder, "MelonLoader.zip");

    [JsonIgnore]
    public string LatestLogPath => GetCombinedPath(MelonLoaderFolder, "Latest.log");

    [JsonIgnore]
    private string Il2CppAssemblyGeneratorFolderPath => Path.Combine(MelonLoaderFolder, "Dependencies", "Il2CppAssemblyGenerator");

    [JsonIgnore]
    public string UnityDependencyZipPath => GetCombinedPath(Il2CppAssemblyGeneratorFolderPath, $"UnityDependencies_{UnityVersion}.zip");

    [JsonIgnore]
    public string Cpp2ILExecutablePath => GetCombinedPath(Il2CppAssemblyGeneratorFolderPath, Path.Combine("Cpp2IL", "Cpp2IL.exe"));

    [JsonIgnore]
    public string Cpp2ILPluginPath => GetCombinedPath(Il2CppAssemblyGeneratorFolderPath, Path.Combine("Cpp2IL", "Plugins", "Cpp2IL.Plugin.StrippedCodeRegSupport.dll"));

    private static string GetCombinedPath(string? folderPath, string targetPath, string defaultPath = "") =>
        !folderPath.IsNullOrEmpty() ? Path.Combine(folderPath, targetPath) : defaultPath;
}