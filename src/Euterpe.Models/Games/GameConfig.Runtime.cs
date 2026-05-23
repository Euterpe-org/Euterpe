using Semver;

namespace Euterpe.Models.Games;

public abstract partial class GameConfig
{
    [JsonIgnore]
    public string GameVersion { get; set; } = string.Empty;

    [JsonIgnore]
    public string UnityVersion { get; set; } = string.Empty;

    [JsonIgnore]
    [ObservableProperty]
    public partial string? MelonLoaderVersion { get; set; }

    [JsonIgnore]
    public SemVersion? MelonLoaderSemVersion { get; private set; }

    [JsonIgnore]
    public string ModsFolder => Path.Combine(Folder, "Mods");

    [JsonIgnore]
    public string UserDataFolder => Path.Combine(Folder, "UserData");

    [JsonIgnore]
    public string UserLibsFolder => Path.Combine(Folder, "UserLibs");

    [JsonIgnore]
    public string GameDataFolder => Path.Combine(Folder, GameDataFolderName);

    [JsonIgnore]
    public string GlobalGameManagersPath => Path.Combine(GameDataFolder, "globalgamemanagers");

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
    public string DotnetRuntimeFolder => Path.Combine(Folder, "dotnet");

    [JsonIgnore]
    public string MelonLoaderDotnetRuntimeFolder => Path.Combine(MelonLoaderFolder, "Dependencies", "dotnet");

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