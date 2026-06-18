namespace Euterpe.Shared.Attributes;

// Routed and registered view models are game-scoped by default; this opts one out into a single app-wide instance.
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class AppSingletonAttribute : Attribute;
