namespace Euterpe.Models.Games;

public sealed class MuseDashConfig : GameConfig
{
    private const WizardOptionKinds Required =
        WizardOptionKinds.MelonLoader
        | WizardOptionKinds.DotNetRuntime
        | WizardOptionKinds.EssentialMods
        | WizardOptionKinds.UninstallConflicts;

    public override GameId Id => GameId.MuseDash;

    public override string DisplayName => "Muse Dash";

    public override string SteamAppId => "774171";

    public override string ExecutableName => "MuseDash.exe";

    public override string GameFolderName => "Muse Dash";

    public override string GameDataFolderName => "MuseDash_Data";

    public override IReadOnlyList<WizardOption> WizardOptions { get; } =
    [
        new(WizardOptionKinds.MelonLoader, Wizard_Task_MelonLoader, Wizard_Task_MelonLoader_Description) { IsSelected = true, IsRequired = true },
        new(WizardOptionKinds.DotNetRuntime, Wizard_Task_DotNetRuntime, Wizard_Task_DotNetRuntime_Description) { IsSelected = true, IsRequired = true },
        new(WizardOptionKinds.EssentialMods, Wizard_Task_EssentialMods, Wizard_Task_EssentialMods_Description) { IsSelected = true, IsRequired = true },
        new(WizardOptionKinds.UninstallConflicts, Wizard_Task_UninstallConflicts, Wizard_Task_UninstallConflicts_Description) { IsSelected = true, IsRequired = true },
        new(WizardOptionKinds.ChartingTool, Wizard_Task_ChartingTool, Wizard_Task_ChartingTool_Description),
        new(WizardOptionKinds.DotNetSdk, Wizard_Task_DotNetSdk, Wizard_Task_DotNetSdk_Description),
        new(WizardOptionKinds.ModTemplate, Wizard_Task_ModTemplate, Wizard_Task_ModTemplate_Description),
        new(WizardOptionKinds.EnvVariable, Wizard_Task_EnvVariable, Wizard_Task_EnvVariable_Description)
    ];

    public override IReadOnlyDictionary<WizardIdentity, WizardOptionKinds> WizardPresets { get; } = new Dictionary<WizardIdentity, WizardOptionKinds>
    {
        [WizardIdentity.Player] = Required,
        [WizardIdentity.Charter] = Required
                                   | WizardOptionKinds.ChartingTool,
        [WizardIdentity.Modder] = Required
                                  | WizardOptionKinds.DotNetSdk
                                  | WizardOptionKinds.ModTemplate
                                  | WizardOptionKinds.EnvVariable
    };
}