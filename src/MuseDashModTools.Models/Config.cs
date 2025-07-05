using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Semver;

namespace MuseDashModTools.Models;

public sealed partial class Config : ObservableObject
{
    // File Management Settings
    [AllowNull]
    [ObservableProperty]
    public partial string MuseDashFolder { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CacheFolder { get; set; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), nameof(MuseDashModTools), "Cache");

    // Appearance Settings
    [AllowNull]
    public string LanguageCode { get; set; } = CultureInfo.CurrentUICulture.ToString();

    public string Theme { get; set; } = "Dark";

    // Experience Settings
    [ObservableProperty]
    public partial bool ShowConsole { get; set; } = true;

    [ObservableProperty]
    public partial bool AlwaysShowScrollBar { get; set; } = true;

    // Download Settings
    [ObservableProperty]
    public partial DownloadSource DownloadSource { get; set; } = DownloadSource.GitHub;

    [ObservableProperty]
    public partial UpdateSource UpdateSource { get; set; } = UpdateSource.GitHubRSS;

    [ObservableProperty]
    public partial string? GitHubToken { get; set; }

    [ObservableProperty]
    public partial string? CustomDownloadSource { get; set; }

    [ObservableProperty]
    public partial bool DownloadPrerelease { get; set; }

    public SemVersion? SkipVersion { get; set; }

    // Advanced Settings
    [ObservableProperty]
    public partial bool IgnoreException { get; set; }

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
    public string UnityDependencyZipPath => GetCombinedPath(Il2CppAssemblyGeneratorFolderPath, "UnityDependencies_2019.4.32.zip");

    [JsonIgnore]
    public string Cpp2ILZipPath => GetCombinedPath(Il2CppAssemblyGeneratorFolderPath, "Cpp2IL_2022.1.0-pre-release.10.zip");

    private static string GetCombinedPath(string? folderPath, string targetPath, string defaultPath = "") =>
        !folderPath.IsNullOrEmpty() ? Path.Combine(folderPath, targetPath) : defaultPath;
}