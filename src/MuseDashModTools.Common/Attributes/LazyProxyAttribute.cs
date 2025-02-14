namespace MuseDashModTools.Common.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class LazyProxyAttribute(Type baseType) : Attribute
{
    public Type BaseType { get; init; } = baseType;
}