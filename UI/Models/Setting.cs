using System.Globalization;
using MemoryPack;
using NuGet.Versioning;

namespace MuseDashModToolsUI.Models;

[MemoryPackable]
public sealed partial class Setting
{
    // Path Settings
    [MemoryPackOrder(0)]
    public string? MuseDashFolder { get; set; } = string.Empty;

    [MemoryPackOrder(1)]
    [SuppressDefaultInitialization]
    public string CacheFolder { get; set; } = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MuseDashModTools");

    // UI Settings
    [MemoryPackOrder(2)]
    [SuppressDefaultInitialization]
    public string? LanguageCode { get; set; } = CultureInfo.CurrentUICulture.ToString();

    [MemoryPackOrder(3)]
    [SuppressDefaultInitialization]
    public string? FontName { get; set; } = FontManageService.DefaultFont;

    [MemoryPackOrder(4)]
    [SuppressDefaultInitialization]
    public string Theme { get; set; } = "Dark";

    // Download Settings
    [MemoryPackOrder(5)]
    [SuppressDefaultInitialization]
    public DownloadSources DownloadSource { get; set; } = DownloadSources.Github;

    [MemoryPackOrder(6)]
    public string? CustomDownloadSource { get; set; } = string.Empty;

    [MemoryPackOrder(7)]
    public bool DownloadPrerelease { get; set; }

    [MemoryPackOrder(8)]
    [MemoryPackAllowSerialize]
    [SuppressDefaultInitialization]
    public SemanticVersion? SkipVersion { get; set; } = SemanticVersion.Parse(AppVersion);

    // Game Settings
    [MemoryPackOrder(9)]
    public bool ShowConsole { get; set; }

    // Message Box Settings
    [MemoryPackOrder(10)]
    public AskType AskInstallMuseDashModTools { get; set; } = AskType.Always;

    [MemoryPackOrder(11)]
    public AskType AskEnableDependencyWhenInstall { get; set; } = AskType.Always;

    [MemoryPackOrder(12)]
    public AskType AskEnableDependencyWhenEnable { get; set; } = AskType.Always;

    [MemoryPackOrder(13)]
    public AskType AskDisableDependencyWhenDelete { get; set; } = AskType.Always;

    [MemoryPackOrder(14)]
    public AskType AskDisableDependencyWhenDisable { get; set; } = AskType.Always;

    // Ignored Paths
    [MemoryPackIgnore]
    public string ModLinksPath => GetCombinedPath(CacheFolder, "ModLinks.json");

    [MemoryPackIgnore]
    public string ChartFolder => GetCombinedPath(CacheFolder, "Charts");

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