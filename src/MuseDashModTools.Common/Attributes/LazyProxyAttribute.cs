namespace MuseDashModTools.Common.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class LazyProxyAttribute(Type baseType) : Attribute;