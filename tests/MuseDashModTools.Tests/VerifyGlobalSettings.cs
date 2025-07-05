using System.Runtime.CompilerServices;

namespace MuseDashModTools.Tests;

public static class VerifyGlobalSettings
{
    [ModuleInitializer]
    public static void Initialize()
    {
        UseProjectRelativeDirectory("snapshots");
        VerifierSettings.AddScrubber(sb => sb.Replace('\\', '/'));
    }
}