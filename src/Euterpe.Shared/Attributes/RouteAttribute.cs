namespace Euterpe.Shared.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class RouteAttribute(string path) : Attribute
{
    public string Path { get; } = path;
    public string DisplayName { get; init; } = "";
    public string Icon { get; init; } = "";
    public int Order { get; init; }
}