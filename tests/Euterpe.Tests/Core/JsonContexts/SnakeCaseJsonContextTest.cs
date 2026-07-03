using System.Reflection;
using Euterpe.Core.Http.Clients;
using Euterpe.Core.JsonContexts;
using Refit;

namespace Euterpe.Tests.Core.JsonContexts;

[Category("SnakeCaseJsonContextTests")]
[TestSubject(typeof(SnakeCaseJsonContext))]
public sealed class SnakeCaseJsonContextTest
{
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
