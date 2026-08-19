using System.Runtime.CompilerServices;

namespace Euterpe.CodeAnalysis.Tests;

public static class VerifyGlobalSettings
{
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifySourceGenerators.Initialize();
        UseProjectRelativeDirectory("snapshots");
        VerifierSettings.AddScrubber(sb => sb.Replace('\\', '/'));
    }
}
