namespace Euterpe.Models.Setup;

[Flags]
public enum SetupOptionKinds
{
    None = 0,
    Migration = 1 << 0,
    MelonLoader = 1 << 1,
    DotNetRuntime = 1 << 2,
    EssentialMods = 1 << 3,
    UninstallConflicts = 1 << 4,
    ChartingTool = 1 << 5,
    DotNetSdk = 1 << 6,
    ModTemplate = 1 << 7,
    EnvVariable = 1 << 8
}
