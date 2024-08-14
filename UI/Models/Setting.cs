using System.Globalization;
using System.Text.Json.Serialization;
using NuGet.Versioning;

namespace MuseDashModToolsUI.Models;

public sealed class Setting
{
    // Path Settings
    [JsonPropertyOrder(0)]
    public string? MuseDashFolder { get; set; } = string.Empty;

    [JsonPropertyOrder(1)]
    public string CacheFolder { get; set; } = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Name);

    // UI Settings
    [JsonPropertyOrder(2)]
    public string? LanguageCode { get; set; } = CultureInfo.CurrentUICulture.ToString();

    [JsonPropertyOrder(3)]
    public string? FontName { get; set; }

    [JsonPropertyOrder(4)]
    public string Theme { get; set; } = "Dark";

    // Download Settings
    [JsonPropertyOrder(5)]
    public DownloadSources DownloadSource { get; set; } = DownloadSources.Github;

    [JsonPropertyOrder(6)]
    public string? CustomDownloadSource { get; set; } = string.Empty;

    [JsonPropertyOrder(7)]
    public bool DownloadPrerelease { get; set; }

    [JsonPropertyOrder(8)]
    public SemanticVersion? SkipVersion { get; set; } = SemanticVersion.Parse(AppVersion);

    // Game Settings
    [JsonPropertyOrder(9)]
    public bool ShowConsole { get; set; }

    // Message Box Settings
    [JsonPropertyOrder(10)]
    public AskType AskInstallMuseDashModTools { get; set; } = AskType.Always;

    [JsonPropertyOrder(11)]
    public AskType AskEnableDependencyWhenInstall { get; set; } = AskType.Always;

    [JsonPropertyOrder(12)]
    public AskType AskEnableDependencyWhenEnable { get; set; } = AskType.Always;

    [JsonPropertyOrder(13)]
    public AskType AskDisableDependencyWhenDelete { get; set; } = AskType.Always;

    [JsonPropertyOrder(14)]
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

    /// <summary>
    ///     Copy values from another Setting
    /// </summary>
    /// <param name="setting"></param>
    public void CopyFrom(Setting setting)
    {
        MuseDashFolder = setting.MuseDashFolder;
        LanguageCode = setting.LanguageCode;
        FontName = setting.FontName;
        SkipVersion = setting.SkipVersion;
        DownloadPrerelease = setting.DownloadPrerelease;
        DownloadSource = setting.DownloadSource;
        CustomDownloadSource = setting.CustomDownloadSource;
        Theme = setting.Theme;
        ShowConsole = setting.ShowConsole;
        AskInstallMuseDashModTools = setting.AskInstallMuseDashModTools;
        AskEnableDependencyWhenInstall = setting.AskEnableDependencyWhenInstall;
        AskEnableDependencyWhenEnable = setting.AskEnableDependencyWhenEnable;
        AskDisableDependencyWhenDelete = setting.AskDisableDependencyWhenDelete;
        AskDisableDependencyWhenDisable = setting.AskDisableDependencyWhenDisable;
    }

    private static string GetCombinedPath(string? folderPath, string targetPath, string defaultPath = "") =>
        !folderPath.IsNullOrEmpty() ? Path.Join(folderPath, targetPath) : defaultPath;
}