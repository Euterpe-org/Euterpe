namespace Euterpe.Models;

public readonly record struct DependencySpec(
    string Name,
    string Url,
    string FilePath,
    string ExpectedHash);