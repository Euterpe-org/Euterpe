namespace Euterpe.Abstractions;

public interface IGamePathService
{
    bool TryGetGameFolderFromVdf(string appId, string relativePath, [NotNullWhen(true)] out string? gameFolder);
    bool TryGetGameFolderFromCommonPaths(string[] commonPaths, string relativePath, [NotNullWhen(true)] out string? gameFolder);
}