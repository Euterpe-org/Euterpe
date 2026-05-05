using System.Diagnostics.CodeAnalysis;
using Semver;

namespace Euterpe.Models.Games;

public abstract partial class GameConfig(
    GameId id,
    string displayName,
    string steamAppId,
    string executableName,
    string gameFolderName,
    string gameDataFolderName) : ObservableObject
{
    [AllowNull]
    [ObservableProperty]
    public partial string Folder { get; set; } = string.Empty;

    [ObservableProperty]
    public partial GameMode GameMode { get; set; } = GameMode.Modded;

    public bool SetupCompleted { get; set; }

    [JsonIgnore]
    public GameId Id { get; } = id;

    [JsonIgnore]
    public string DisplayName { get; } = displayName;

    [JsonIgnore]
    public string SteamAppId { get; } = steamAppId;

    [JsonIgnore]
    public string ExecutableName { get; } = executableName;

    [JsonIgnore]
    public string GameFolderName { get; } = gameFolderName;

    [JsonIgnore]
    public string GameDataFolderName { get; } = gameDataFolderName;

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
    [JsonIgnore]
    public string ModsFolder => Path.Combine(Folder, "Mods");

    [JsonIgnore]
    public string UserDataFolder => Path.Combine(Folder, "UserData");

    [JsonIgnore]
    public string UserLibsFolder => Path.Combine(Folder, "UserLibs");

    [JsonIgnore]
    public string GameDataFolder => Path.Combine(Folder, GameDataFolderName);

    [JsonIgnore]
    public string OnlineChartsFolder => Path.Combine(EuterpeChartsFolder, "Online");

    [JsonIgnore]
    public string OfflineChartsFolder => Path.Combine(EuterpeChartsFolder, "Offline");

    [JsonIgnore]
    public string MelonLoaderFolder => Path.Combine(Folder, "MelonLoader");

    [JsonIgnore]
    public string MelonLoaderZipPath => Path.Combine(Folder, "MelonLoader.zip");

    [JsonIgnore]
    public string LatestLogPath => Path.Combine(MelonLoaderFolder, "Latest.log");

    [JsonIgnore]
    public string UnityDependencyZipPath => Path.Combine(Il2CppAssemblyGeneratorFolderPath, $"UnityDependencies_{UnityVersion}.zip");

    [JsonIgnore]
    public string Cpp2ILExecutablePath => Path.Combine(Il2CppAssemblyGeneratorFolderPath, "Cpp2IL", "Cpp2IL.exe");

    [JsonIgnore]
    public string Cpp2ILPluginPath => Path.Combine(Il2CppAssemblyGeneratorFolderPath, "Cpp2IL", "Plugins", "Cpp2IL.Plugin.StrippedCodeRegSupport.dll");

    private string EuterpeChartsFolder => Path.Combine(Folder, "Euterpe_Charts");

    private string Il2CppAssemblyGeneratorFolderPath => Path.Combine(MelonLoaderFolder, "Dependencies", "Il2CppAssemblyGenerator");

    partial void OnMelonLoaderVersionChanged(string? value) =>
        MelonLoaderSemVersion = SemVersion.TryParse(value, out var version) ? version : null;
}