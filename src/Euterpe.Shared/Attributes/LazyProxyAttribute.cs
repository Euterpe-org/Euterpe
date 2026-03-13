#pragma warning disable CS9113 // Parameter is unread.
namespace Euterpe.Shared.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class LazyProxyAttribute(Type baseType) : Attribute;