using System.Globalization;
using System.Text.Json.Serialization;
using NuGet.Versioning;

namespace MuseDashModToolsUI.Models;

public sealed class Setting
{
    public string? MuseDashFolder { get; set; }
    public string? LanguageCode { get; set; } = CultureInfo.CurrentUICulture.ToString();
    public string? FontName { get; set; } = FontManageService.DefaultFont;
    public SemanticVersion? SkipVersion { get; set; } = SemanticVersion.Parse(AppVersion);
    public bool DownloadPrerelease { get; set; }
    public DownloadSources DownloadSource { get; set; } = DownloadSources.Github;
    public string? CustomDownloadSource { get; set; }
    public string Theme { get; set; } = "Dark";
    public bool ShowConsole { get; set; }
    public AskType AskInstallMuseDashModTools { get; set; } = AskType.Always;
    public AskType AskEnableDependencyWhenInstall { get; set; } = AskType.Always;
    public AskType AskEnableDependencyWhenEnable { get; set; } = AskType.Always;
    public AskType AskDisableDependencyWhenDelete { get; set; } = AskType.Always;
    public AskType AskDisableDependencyWhenDisable { get; set; } = AskType.Always;

    [JsonIgnore]
    public string CustomAlbumsFolder =>
        !string.IsNullOrEmpty(MuseDashFolder) ? Path.Join(MuseDashFolder, "Custom_Albums") : string.Empty;

    [JsonIgnore]
    public string MuseDashExePath =>
        !string.IsNullOrEmpty(MuseDashFolder) ? Path.Join(MuseDashFolder, "MuseDash.exe") : string.Empty;

    [JsonIgnore]
    public string UserDataFolder =>
        !string.IsNullOrEmpty(MuseDashFolder) ? Path.Join(MuseDashFolder, "UserData") : string.Empty;

    [JsonIgnore]
    public string ModsFolder =>
        !string.IsNullOrEmpty(MuseDashFolder) ? Path.Join(MuseDashFolder, "Mods") : string.Empty;

    [JsonIgnore]
    public string MelonLoaderFolder =>
        !string.IsNullOrEmpty(MuseDashFolder) ? Path.Join(MuseDashFolder, "MelonLoader") : string.Empty;

    [JsonIgnore]
    public string MelonLoaderZipPath =>
        !string.IsNullOrEmpty(MuseDashFolder) ? Path.Join(MuseDashFolder, "MelonLoader.zip") : string.Empty;

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
}