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
        builder.RegisterCoreServices();

        await using var container = builder.Build();

        var registrations = container.ComponentRegistry
            .RegistrationsFor(new TypedService(typeof(IWizardStep)))
            .ToList();

        await Assert.That(registrations.Count).IsEqualTo(7);

        var implTypes = registrations
            .Select(r => r.Activator.LimitType.Name)
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToList();

        var expected = new[]
        {
            "ChartingToolStep",
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