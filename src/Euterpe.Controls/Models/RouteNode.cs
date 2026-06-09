namespace Euterpe.Controls.Models;

public sealed record RouteNode(string Path, Action? Select, RouteNode[] Children);
