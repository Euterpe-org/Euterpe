namespace Euterpe.Models.Games;

public abstract partial class GameConfig
{
    [JsonIgnore]
    public abstract GameId Id { get; }

    [JsonIgnore]
    public abstract string DisplayName { get; }

    [JsonIgnore]
    public abstract string SteamAppId { get; }

    [JsonIgnore]
    public abstract string ExecutableName { get; }

    [JsonIgnore]
    public abstract string GameFolderName { get; }

    [JsonIgnore]
    public abstract string GameDataFolderName { get; }

    [JsonIgnore]
    public abstract string ModTemplatePackageName { get; }

    [JsonIgnore]
    public abstract string ModTemplateShortName { get; }

    [JsonIgnore]
    public abstract string PathEnvironmentVariableName { get; }

    [JsonIgnore]
    public abstract IReadOnlyList<SetupOption> SetupOptions { get; }

    [JsonIgnore]
    public abstract IReadOnlyDictionary<WizardIdentity, SetupOptionKinds> WizardPresets { get; }
}
