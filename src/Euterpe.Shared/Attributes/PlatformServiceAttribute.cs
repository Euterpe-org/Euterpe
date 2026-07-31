using Euterpe.Shared.DependencyInjection;

namespace Euterpe.Shared.Attributes;

[AttributeUsage(AttributeTargets.Interface)]
public sealed class PlatformServiceAttribute(ServiceRegistrationLifetime lifetime = ServiceRegistrationLifetime.PerGame) : Attribute
{
    public ServiceRegistrationLifetime Lifetime { get; } = lifetime;
}
