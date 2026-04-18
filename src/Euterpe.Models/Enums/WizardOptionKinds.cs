namespace Euterpe.Models.Enums;

[Flags]
public enum WizardOptionKinds
{
    None = 0,
    MelonLoader = 1 << 0,
    EssentialMods = 1 << 1,
    UninstallConflicts = 1 << 2,
    ChartingTool = 1 << 3,
    DotNetSdk = 1 << 4,
    ModTemplate = 1 << 5,
    EnvVariable = 1 << 6
}