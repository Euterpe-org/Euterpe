namespace Euterpe.Styles.Models;

public sealed record RouteNode(string Path, Action? Select, RouteNode[] Children);