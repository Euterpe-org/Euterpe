using System.Runtime.CompilerServices;

namespace MuseDashModTools.Tests;

public static class VerifyGlobalSettings
{
    [ModuleInitializer]
    public static void Initialize()
    {
        UseProjectRelativeDirectory("snapshots");
        // Make path separators consistent across platforms
        VerifierSettings.AddScrubber(sb => sb.Replace('\\', '/'));
    }
}