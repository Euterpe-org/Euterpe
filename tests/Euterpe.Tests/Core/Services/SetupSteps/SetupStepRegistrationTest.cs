using Autofac;
using Autofac.Core;
using Euterpe.Core.Extensions;

namespace Euterpe.Tests.Core;

[Category("SetupStepRegistrationTests")]
[TestSubject(typeof(ISetupStep))]
public sealed class SetupStepRegistrationTest
{
    [Test]
    public async Task AllSetupSteps_Registered_AsISetupStep()
    {
        var builder = new ContainerBuilder();
        builder.RegisterAppCoreServices();
        builder.RegisterPerGameCoreServices(GameId.MuseDash);

        await using var container = builder.Build();

        var registrations = container.ComponentRegistry
            .RegistrationsFor(new TypedService(typeof(ISetupStep)))
            .ToArray();

        var implTypes = registrations
            .Select(r => r.Activator.LimitType.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();

        string[] expected =
        [
            "ChartingToolStep",
            "DotNetRuntimeStep",
            "DotNetSdkStep",
            "EnvVariableStep",
            "EssentialModsStep",
            "MelonLoaderStep",
            "MigrationStep",
            "ModTemplateStep",
            "UninstallConflictsStep"
        ];

        await Assert.That(implTypes).IsEquivalentTo(expected, StringComparer.Ordinal, CollectionOrdering.Matching);
    }
}
