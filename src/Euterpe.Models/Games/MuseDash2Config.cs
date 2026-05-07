namespace Euterpe.Models.Games;

public sealed class MuseDash2Config : GameConfig
{
    private const SetupOptionKinds Required =
        SetupOptionKinds.MelonLoader
        | SetupOptionKinds.DotNetRuntime
        | SetupOptionKinds.EssentialMods
        | SetupOptionKinds.UninstallConflicts;

    [JsonIgnore]
    public override GameId Id => GameId.MuseDash2;

    [JsonIgnore]
    public override string DisplayName => "Muse Dash 2";

    [JsonIgnore]
    public override string SteamAppId => "0";

    [JsonIgnore]
    public override string ExecutableName => "MuseDash2.exe";

    [JsonIgnore]
    public override string GameFolderName => "Muse Dash 2";

    [JsonIgnore]
    public override string GameDataFolderName => "MuseDash2_Data";

    [JsonIgnore]
    public override string UidRegistryValueName => string.Empty;

    [JsonIgnore]
    public override string ModTemplatePackageName => "MuseDash2.Mod.Template";

    [JsonIgnore]
    public override string ModTemplateShortName => "musedash2mod";

    [JsonIgnore]
    public override string PathEnvironmentVariableName => "MD2_DIRECTORY";

    [JsonIgnore]
    public override IReadOnlyList<SetupOption> SetupOptions { get; } =
    [
        new(SetupOptionKinds.MelonLoader, Wizard_Task_MelonLoader, Wizard_Task_MelonLoader_Description) { IsSelected = true, IsRequired = true },
        new(SetupOptionKinds.DotNetRuntime, Wizard_Task_DotNetRuntime, Wizard_Task_DotNetRuntime_Description) { IsSelected = true, IsRequired = true },
        new(SetupOptionKinds.EssentialMods, Wizard_Task_EssentialMods, Wizard_Task_EssentialMods_Description) { IsSelected = true, IsRequired = true },
        new(SetupOptionKinds.UninstallConflicts, Wizard_Task_UninstallConflicts, Wizard_Task_UninstallConflicts_Description) { IsSelected = true, IsRequired = true },
        new(SetupOptionKinds.ChartingTool, Wizard_Task_ChartingTool, Wizard_Task_ChartingTool_Description),
        new(SetupOptionKinds.DotNetSdk, Wizard_Task_DotNetSdk, Wizard_Task_DotNetSdk_Description),
        new(SetupOptionKinds.ModTemplate, Wizard_Task_ModTemplate, Wizard_Task_ModTemplate_Description),
        new(SetupOptionKinds.EnvVariable, Wizard_Task_EnvVariable, Wizard_Task_EnvVariable_Description)
    ];

    [JsonIgnore]
    public override IReadOnlyDictionary<WizardIdentity, SetupOptionKinds> WizardPresets { get; } = new Dictionary<WizardIdentity, SetupOptionKinds>
    {
        [WizardIdentity.Player] = Required,
        [WizardIdentity.Charter] = Required
                                   | SetupOptionKinds.ChartingTool,
        [WizardIdentity.Modder] = Required
                                  | SetupOptionKinds.DotNetSdk
                                  | SetupOptionKinds.ModTemplate
                                  | SetupOptionKinds.EnvVariable
    };
}