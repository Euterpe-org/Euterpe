using Semver;

namespace Euterpe.Tests.Attributes;

public sealed class PrereleaseOnlyAttribute() : SkipAttribute("This test is only for prerelease versions.")
{
    public override Task<bool> ShouldSkip(TestRegisteredContext context) =>
        Task.FromResult(!SemVersion.Parse(BuildInfo.AppVersion).IsPrerelease);
}