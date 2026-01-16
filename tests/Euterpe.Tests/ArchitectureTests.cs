using NetArchTest.Rules;

namespace Euterpe.Tests;

[Category("ArchitectureTests")]
public sealed class ArchitectureTests
{
    private static readonly Types Types = Types.InCurrentDomain();

    [Test]
    public async Task Abstractions_ClassesArePublicAndInterfaces_ReturnsTrue()
    {
        var result = Types.That()
            .ResideInNamespace("Euterpe.Abstractions")
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
            .ResideInNamespaceMatching("^Euterpe.Core$")
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
            .ResideInNamespace("Euterpe.Common")
            .Should()
            .BePublic()
            .GetResult();

        await Assert.That(result.IsSuccessful).IsTrue();
    }

    [Test]
    public async Task Models_ClassesArePublic_ReturnsTrue()
    {
        var result = Types.That()
            .ResideInNamespace("Euterpe.Models")
            .And()
            .AreNotStatic()
            .Should()
            .BePublic()
            .GetResult();

        await Assert.That(result.IsSuccessful).IsTrue();
    }
}