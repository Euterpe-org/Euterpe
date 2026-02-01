using NetArchTest.Rules;
using Assembly = System.Reflection.Assembly;

namespace Euterpe.Tests;

[Category("ArchitectureTests")]
public sealed class ArchitectureTests
{
    private static readonly Types AbstractionsTypes = Types.InAssembly(Assembly.Load("Euterpe.Abstractions"));

    private static readonly Types CoreTypes = Types.InAssembly(Assembly.Load("Euterpe.Core"));

    private static readonly Types CommonTypes = Types.InAssembly(Assembly.Load("Euterpe.Common"));

    private static readonly Types ModelsTypes = Types.InAssembly(Assembly.Load("Euterpe.Models"));

    [Test]
    public async Task Abstractions_ClassesArePublicAndInterfaces_ReturnsTrue()
    {
        var result = AbstractionsTypes.That()
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
        var result = CoreTypes.That()
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
        var result = CommonTypes.That()
            .ResideInNamespace("Euterpe.Common")
            .Should()
            .BePublic()
            .GetResult();

        await Assert.That(result.IsSuccessful).IsTrue();
    }

    [Test]
    public async Task Models_ClassesArePublic_ReturnsTrue()
    {
        var result = ModelsTypes.That()
            .ResideInNamespace("Euterpe.Models")
            .And()
            .AreNotStatic()
            .Should()
            .BePublic()
            .GetResult();

        await Assert.That(result.IsSuccessful).IsTrue();
    }
}