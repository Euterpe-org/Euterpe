namespace Euterpe.Abstractions;

public interface IPlatformLauncher
{
    /// <summary>
    ///     Open File
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    Task OpenFileAsync(string filePath);

    /// <summary>
    ///     Open Folder
    /// </summary>
    /// <param name="folderPath"></param>
    /// <returns></returns>
    Task OpenFolderAsync(string folderPath);

    /// <summary>
    ///     Open Uri
    /// </summary>
    /// <param name="uri"></param>
    /// <returns></returns>
    Task OpenUriAsync(string uri);

    /// <summary>
    ///     Reveal file with path
    /// </summary>
    /// <param name="filePath"></param>
    Task RevealFileAsync(string filePath);
}
