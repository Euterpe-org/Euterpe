using Autofac;
using Autofac.Core;
using Euterpe.Core.Extensions;

namespace Euterpe.Tests;

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

        await Assert.That(registrations.Length).IsEqualTo(9);

        var implTypes = registrations
            .Select(r => r.Activator.LimitType.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();

        var expected = new[]
        {
            "ChartingToolStep",
            "DotNetRuntimeStep",
            "DotNetSdkStep",
            "EnvVariableStep",
            "EssentialModsStep",
            "MelonLoaderStep",
            "MigrationStep",
            "ModTemplateStep",
            "UninstallConflictsStep"
        };

        for (var i = 0; i < expected.Length; i++)
        {
            await Assert.That(implTypes[i]).IsEqualTo(expected[i]);
        }
    }
}