#pragma warning disable CS9113 // Parameter is unread.
namespace MuseDashModTools.Common.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class LazyProxyAttribute(Type baseType) : Attribute;