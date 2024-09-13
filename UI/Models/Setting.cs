using System.Globalization;
using System.Text.Json.Serialization;
using Mapster;

namespace MuseDashModToolsUI.Models;

public sealed class Setting
{
    // Path Settings
    [JsonPropertyOrder(0)]
    public string? MuseDashFolder { get; set; } = "C:\\Users\\32626\\Desktop\\test";

    [JsonPropertyOrder(1)]
    public string CacheFolder { get; set; } = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Name);

    // UI Settings
    [JsonPropertyOrder(2)]
    public string? LanguageCode { get; set; } = CultureInfo.CurrentUICulture.ToString();

    [JsonPropertyOrder(3)]
    public string Theme { get; set; } = "Dark";

    // Download Settings
    [JsonPropertyOrder(4)]
    public DownloadSource DownloadSource { get; set; } = DownloadSource.GitHub;

    [JsonPropertyOrder(5)]
    public string? CustomDownloadSource { get; set; } = string.Empty;

    [JsonPropertyOrder(6)]
    public bool DownloadPrerelease { get; set; }

    [JsonPropertyOrder(7)]
    public SemVersion? SkipVersion { get; set; }

    // Game Settings
    [JsonPropertyOrder(8)]
    public bool ShowConsole { get; set; }

    // Message Box Settings
    [JsonPropertyOrder(9)]
    public AskType AskInstallMuseDashModTools { get; set; } = AskType.Always;

    [JsonPropertyOrder(10)]
    public AskType AskEnableDependencyWhenInstall { get; set; } = AskType.Always;

    [JsonPropertyOrder(11)]
    public AskType AskEnableDependencyWhenEnable { get; set; } = AskType.Always;

    [JsonPropertyOrder(12)]
    public AskType AskDisableDependencyWhenDelete { get; set; } = AskType.Always;

    [JsonPropertyOrder(13)]
    public AskType AskDisableDependencyWhenDisable { get; set; } = AskType.Always;

    // Ignored Paths
    [JsonIgnore]
    public string ModLinksPath => GetCombinedPath(CacheFolder, "ModLinks.json");

    [JsonIgnore]
    public string ChartFolder => GetCombinedPath(CacheFolder, "Charts");

    [JsonIgnore]
    public string CustomAlbumsFolder => GetCombinedPath(MuseDashFolder, "Custom_Albums");

    [JsonIgnore]
    public string MuseDashExePath => GetCombinedPath(MuseDashFolder, "MuseDash.exe");

    [JsonIgnore]
    public string UserDataFolder => GetCombinedPath(MuseDashFolder, "UserData");

    [JsonIgnore]
    public string ModsFolder => GetCombinedPath(MuseDashFolder, "Mods");

    [JsonIgnore]
    public string MelonLoaderFolder => GetCombinedPath(MuseDashFolder, "MelonLoader");

    [JsonIgnore]
    public string MelonLoaderZipPath => GetCombinedPath(MuseDashFolder, "MelonLoader.zip");

    [JsonIgnore]
    private string Il2CppAssemblyGeneratorFolderPath => Path.Join(MelonLoaderFolder, "Dependencies", "Il2CppAssemblyGenerator");

    [JsonIgnore]
    public string UnityDependencyZipPath => GetCombinedPath(Il2CppAssemblyGeneratorFolderPath, "UnityDependencies_2019.4.32.zip");

    [JsonIgnore]
    public string Cpp2ILZipPath => GetCombinedPath(Il2CppAssemblyGeneratorFolderPath, "Cpp2IL_2022.1.0-pre-release.10.zip");

    public void CopyFrom(Setting setting) => setting.Adapt(this);

    private static string GetCombinedPath(string? folderPath, string targetPath, string defaultPath = "") =>
        !folderPath.IsNullOrEmpty() ? Path.Join(folderPath, targetPath) : defaultPath;
}