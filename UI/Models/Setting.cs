using System.Globalization;
using MemoryPack;
using NuGet.Versioning;

namespace MuseDashModToolsUI.Models;

[MemoryPackable]
public sealed partial class Setting
{
    public string? MuseDashFolder { get; set; } = string.Empty;

    public string ConfigFolder { get; set; } =
        Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MuseDashModTools");

    public string? LanguageCode { get; set; } = CultureInfo.CurrentUICulture.ToString();
    public string? FontName { get; set; } = FontManageService.DefaultFont;

    [MemoryPackAllowSerialize]
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

    [MemoryPackIgnore]
    public string ModLinksPath => GetCombinedPath(ConfigFolder, "ModLinks.json");

    [MemoryPackIgnore]
    public string ChartFolder => GetCombinedPath(ConfigFolder, "Charts");

    [MemoryPackIgnore]
    public string CustomAlbumsFolder => GetCombinedPath(MuseDashFolder, "Custom_Albums");

    [MemoryPackIgnore]
    public string MuseDashExePath => GetCombinedPath(MuseDashFolder, "MuseDash.exe");

    [MemoryPackIgnore]
    public string UserDataFolder => GetCombinedPath(MuseDashFolder, "UserData");

    [MemoryPackIgnore]
    public string ModsFolder => GetCombinedPath(MuseDashFolder, "Mods");

    [MemoryPackIgnore]
    public string MelonLoaderFolder => GetCombinedPath(MuseDashFolder, "MelonLoader");

    [MemoryPackIgnore]
    public string MelonLoaderZipPath => GetCombinedPath(MuseDashFolder, "MelonLoader.zip");

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

    private static string GetCombinedPath(string? folderPath, string targetPath, string defaultPath = "") =>
        !string.IsNullOrEmpty(folderPath) ? Path.Join(folderPath, targetPath) : defaultPath;
}