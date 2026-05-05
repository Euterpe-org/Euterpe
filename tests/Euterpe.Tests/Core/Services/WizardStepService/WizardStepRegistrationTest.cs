using Autofac;
using Autofac.Core;
using Euterpe.Core.Extensions;

namespace Euterpe.Tests;

[Category("WizardStepRegistrationTests")]
[TestSubject(typeof(IWizardStep))]
public sealed class WizardStepRegistrationTest
{
    [Test]
    public async Task AllWizardSteps_Registered_AsIWizardStep()
    {
        var builder = new ContainerBuilder();
        builder.RegisterAppCoreServices();
        builder.RegisterPerGameCoreServices(GameId.MuseDash);

        await using var container = builder.Build();

        var registrations = container.ComponentRegistry
            .RegistrationsFor(new TypedService(typeof(IWizardStep)))
            .ToArray();

        await Assert.That(registrations.Length).IsEqualTo(8);

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
            "ModTemplateStep",
            "UninstallConflictsStep"
        };

        for (var i = 0; i < expected.Length; i++)
        {
            await Assert.That(implTypes[i]).IsEqualTo(expected[i]);
        }
    }
}