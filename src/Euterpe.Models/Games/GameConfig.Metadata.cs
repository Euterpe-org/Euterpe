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
    public abstract string UidRegistryValueName { get; }

    [JsonIgnore]
    public abstract IReadOnlyList<WizardOption> WizardOptions { get; }

    [JsonIgnore]
    public abstract IReadOnlyDictionary<WizardIdentity, WizardOptionKinds> WizardPresets { get; }
}