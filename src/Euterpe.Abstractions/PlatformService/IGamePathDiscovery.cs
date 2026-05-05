namespace Euterpe.Abstractions;

public interface IGamePathDiscovery
{
    /// <summary>
    ///     Get game folder path
    /// </summary>
    /// <param name="gameFolder"></param>
    /// <returns>Is success</returns>
    bool TryGetGameFolder([NotNullWhen(true)] out string? gameFolder);

    /// <summary>
    ///     Check is valid game folder
    /// </summary>
    /// <param name="folderPath"></param>
    /// <returns></returns>
    bool CheckIsValidGameFolder(string folderPath);
}