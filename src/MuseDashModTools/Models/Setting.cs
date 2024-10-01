using System.Text.Json.Serialization;
using Mapster;

namespace MuseDashModTools.Models;

public sealed class Setting
{
    // Path Settings
    [AllowNull]
    public string MuseDashFolder { get; set; } = string.Empty;

    public string CacheFolder { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppName, "Cache");

    // UI Settings
    [AllowNull]
    public string LanguageCode { get; set; } = CultureInfo.CurrentUICulture.ToString();

    public string Theme { get; set; } = "Dark";

    // Download Settings
    public DownloadSource DownloadSource { get; set; } = DownloadSource.GitHub;
    public string? GitHubToken { get; set; } = null;

    public string? CustomDownloadSource { get; set; } = null;
    public bool DownloadPrerelease { get; set; }
    public SemVersion? SkipVersion { get; set; }

    // Game Settings
    public bool ShowConsole { get; set; }

    // Message Box Settings
    public AskType AskEnableDependencyWhenEnableCurrent { get; set; } = AskType.Always;
    public AskType AskDisableDependentWhenDisableCurrent { get; set; } = AskType.Always;

    // Ignored Paths
    [JsonIgnore]
    public string ModLinksPath => GetCombinedPath(CacheFolder, "ModLinks.json");

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

    public void CopyFrom(Setting setting) => setting.Adapt(this);

    private static string GetCombinedPath(string? folderPath, string targetPath, string defaultPath = "") =>
        !folderPath.IsNullOrEmpty() ? Path.Combine(folderPath, targetPath) : defaultPath;
}