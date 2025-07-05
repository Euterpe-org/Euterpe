using NetArchTest.Rules;

namespace MuseDashModTools.Tests;

public sealed class ArchTests
{
    private static readonly Types Types = Types.InCurrentDomain();

    [Test]
    public async Task Abstractions_ShouldBePublicAndInterfaces()
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
    public async Task CoreServices_ShouldBeInternalAndSealed()
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
    public async Task Common_ShouldBePublic()
    {
        var result = Types.That()
            .ResideInNamespace("MuseDashModTools.Common")
            .Should()
            .BePublic()
            .GetResult();

        await Assert.That(result.IsSuccessful).IsTrue();
    }

    [Test]
    public async Task Models_ShouldBePublic()
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