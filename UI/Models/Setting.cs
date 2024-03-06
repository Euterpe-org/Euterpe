using System.Globalization;
using System.Text.Json.Serialization;
using NuGet.Versioning;

namespace MuseDashModToolsUI.Models;

public sealed class Setting
{
    public string? MuseDashFolder { get; set; } = string.Empty;

    public string ConfigFolder { get; set; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MuseDashModTools");

    public string? LanguageCode { get; set; } = CultureInfo.CurrentUICulture.ToString();
    public string? FontName { get; set; } = FontManageService.DefaultFont;
    public SemanticVersion? SkipVersion { get; set; } = SemanticVersion.Parse(AppVersion);
    public bool DownloadPrerelease { get; set; }
    public DownloadSources DownloadSource { get; set; } = DownloadSources.Github;
    public string? CustomDownloadSource { get; set; } = string.Empty;
    public string Theme { get; set; } = "Dark";
    public bool ShowConsole { get; set; }
    public AskType AskInstallMuseDashModTools { get; set; } = AskType.Always;
    public AskType AskEnableDependencyWhenInstall { get; set; } = AskType.Always;
    public AskType AskEnableDependencyWhenEnable { get; set; } = AskType.Always;
    public AskType AskDisableDependencyWhenDelete { get; set; } = AskType.Always;
    public AskType AskDisableDependencyWhenDisable { get; set; } = AskType.Always;

    [JsonIgnore]
    public string ModLinksPath => Path.Combine(ConfigFolder, "ModLinks.json");

    [JsonIgnore]
    public string ChartFolder => Path.Combine(ConfigFolder, "Charts");

    [JsonIgnore]
    public string CustomAlbumsFolder => GetCombinedPath("Custom_Albums");

    [JsonIgnore]
    public string MuseDashExePath => GetCombinedPath("MuseDash.exe");

    [JsonIgnore]
    public string UserDataFolder => GetCombinedPath("UserData");

    [JsonIgnore]
    public string ModsFolder => GetCombinedPath("Mods");

    [JsonIgnore]
    public string MelonLoaderFolder => GetCombinedPath("MelonLoader");

    [JsonIgnore]
    public string MelonLoaderZipPath => GetCombinedPath("MelonLoader.zip");

    /// <summary>
    ///     Copy values from another Setting
    /// </summary>
    /// <param name="setting"></param>
    public void Copy(Setting setting)
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

    private string GetCombinedPath(string path) =>
        !string.IsNullOrEmpty(MuseDashFolder) ? Path.Join(MuseDashFolder, path) : string.Empty;
}