namespace Euterpe.Abstractions;

public interface ISteamPathDiscovery
{
    /// <summary>
    ///     Get steam folder path
    /// </summary>
    /// <param name="steamFolder"></param>
    /// <returns>Is success</returns>
    bool TryGetSteamFolder([NotNullWhen(true)] out string? steamFolder);

    /// <summary>
    ///     Check is valid Steam folder
    /// </summary>
    /// <param name="folderPath"></param>
    /// <returns></returns>
    bool CheckIsValidSteamFolder(string folderPath);

    /// <summary>
    ///     Get steam executable path
    /// </summary>
    /// <returns></returns>
    Task<string?> GetSteamExecPathAsync();

    /// <summary>
    ///     Check is valid Steam executable path
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    bool CheckIsValidSteamExecPath(string filePath);
}
