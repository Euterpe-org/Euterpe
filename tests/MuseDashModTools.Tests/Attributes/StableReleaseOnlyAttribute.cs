using Semver;

namespace MuseDashModTools.Tests.Attributes;

public sealed class StableReleaseOnlyAttribute() : SkipAttribute("This test is only for stable releases")
{
    public override Task<bool> ShouldSkip(TestRegisteredContext context) =>
        Task.FromResult(SemVersion.Parse(BuildInfo.AppVersion).IsPrerelease);
}