namespace Euterpe.Models.Dependencies;

public readonly record struct DependencySpec(string Name, string Url, string FilePath, string ExpectedHash);