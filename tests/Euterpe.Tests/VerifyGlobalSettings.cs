using System.Runtime.CompilerServices;

namespace Euterpe.Tests;

public static class VerifyGlobalSettings
{
    [ModuleInitializer]
    public static void Initialize()
    {
        UseProjectRelativeDirectory("snapshots");
        // Make path separators consistent across platforms
        VerifierSettings.AddScrubber(sb => sb.Replace('\\', '/'));

        VerifierSettings.IgnoreMembers<Config>(nameof(Config.Games), nameof(Config.ActiveGameConfig));
        VerifierSettings.IgnoreMembers<GameConfig>(nameof(GameConfig.SetupOptions), nameof(GameConfig.WizardPresets));
    }
}