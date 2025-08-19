using NetArchTest.Rules;

namespace MuseDashModTools.Tests;

[Category("ArchitectureTests")]
public sealed class ArchitectureTests
{
    private static readonly Types Types = Types.InCurrentDomain();

    [Test]
    public async Task Abstractions_ClassesArePublicAndInterfaces_ReturnsTrue()
    {
        var result = Types.That()
            .ResideInNamespace("MuseDashModTools.Abstractions")
            .Should()
            .BePublic()
            .And()
            .BeInterfaces()
            .GetResult();

        await Assert.That(result.IsSuccessful).IsTrue();
    }

    [Test]
    public async Task CoreServices_ClassesAreInternalAndSealed_ReturnsTrue()
    {
        var result = Types.That()
            .ResideInNamespaceMatching("^MuseDashModTools.Core$")
            .Should()
            .BeInternal()
            .And()
            .BeSealed()
            .GetResult();

        await Assert.That(result.IsSuccessful).IsTrue();
    }

    [Test]
    public async Task Common_ClassesArePublic_ReturnsTrue()
    {
        var result = Types.That()
            .ResideInNamespace("MuseDashModTools.Common")
            .Should()
            .BePublic()
            .GetResult();

        await Assert.That(result.IsSuccessful).IsTrue();
    }

    [Test]
    public async Task Models_ClassesArePublic_ReturnsTrue()
    {
        var result = Types.That()
            .ResideInNamespace("MuseDashModTools.Models")
            .And()
            .AreNotStatic()
            .Should()
            .BePublic()
            .GetResult();

        await Assert.That(result.IsSuccessful).IsTrue();
    }
}