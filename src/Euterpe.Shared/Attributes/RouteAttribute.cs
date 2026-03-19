#pragma warning disable CS9113 // Parameter is unread.
namespace Euterpe.Shared.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class RouteAttribute(string path) : Attribute
{
    public string Path { get; } = path;
    public string DisplayName { get; set; } = "";
    public string Icon { get; set; } = "";
    public int Order { get; set; }
}
