using System.Reflection;
using System.Text.Json;
using Euterpe.Contracts.Distribution;
using Euterpe.Core.Http.Clients;
using Euterpe.Core.JsonContexts;
using Refit;

namespace Euterpe.Tests.Core.JsonContexts;

[Category("SnakeCaseJsonContextTests")]
[TestSubject(typeof(SnakeCaseJsonContext))]
public sealed class SnakeCaseJsonContextTest
{
    [Test]
    public async Task DependencyMetadata_RoundTrip_UsesExactDotnetRuntimeVersionKey()
    {
        Dependency[] dependencies =
        [
            new()
            {
                Slug = "MelonLoader",
                Versions = new Dictionary<string, DistributionVersion<DependencyMetadata>>
                {
                    ["0.7.3"] = new()
                    {
                        Metadata = new DependencyMetadata { DotNetRuntimeVersion = "6.0" }
                    }
                }
            }
        ];

        var json = JsonSerializer.Serialize(dependencies, SnakeCaseJsonContext.Default.DependencyArray);
        var roundTripped = JsonSerializer.Deserialize(json, SnakeCaseJsonContext.Default.DependencyArray)!;

        using var _ = Assert.Multiple();
        await Assert.That(json).Contains("\"dotnet_runtime_version\":\"6.0\"");
        await Assert.That(json.Contains("\"dot_net_runtime_version\"", StringComparison.Ordinal)).IsFalse();
        await Assert.That(roundTripped.Single().Versions.Single().Value.Metadata.DotNetRuntimeVersion).IsEqualTo("6.0");
    }

    [Test]
    public async Task DependencyMetadata_NumberRuntimeVersion_ThrowsJsonException()
    {
        const string json = """
                            [{"slug":"MelonLoader","versions":{"0.7.3":{"metadata":{"dotnet_runtime_version":6.0}}}}]
                            """;

        var act = () => JsonSerializer.Deserialize(json, SnakeCaseJsonContext.Default.DependencyArray);

        await Assert.That(act).Throws<JsonException>();
    }

    [Test]
    public async Task GetTypeInfo_EveryRefitClientWireType_IsRegistered()
    {
        var wireTypes = typeof(IEuterpeAuthClient).Assembly.GetTypes()
            .Where(type => type.IsInterface && type.Namespace == typeof(IEuterpeAuthClient).Namespace)
            .SelectMany(type => type.GetMethods())
            .SelectMany(WireTypes)
            .Where(type => type != typeof(HttpResponseMessage))
            .Distinct();

        var unregistered = wireTypes.Where(type => SnakeCaseJsonContext.Default.GetTypeInfo(type) is null).ToArray();

        await Assert.That(unregistered).IsEmpty();
    }

    private static IEnumerable<Type> WireTypes(MethodInfo method)
    {
        if (method.ReturnType is { IsGenericType: true } taskType && taskType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            yield return taskType.GenericTypeArguments[0];
        }

        foreach (var parameter in method.GetParameters())
        {
            if (parameter.GetCustomAttribute<BodyAttribute>() is not null)
            {
                yield return parameter.ParameterType;
            }
        }
    }
}
