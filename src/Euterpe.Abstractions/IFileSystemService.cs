namespace Euterpe.Abstractions;

public interface IFileSystemService
{
    /// <summary>
    ///     Provides logging besides normal checking
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    bool CheckFileExists(string filePath);

    bool TryDeleteFile(string filePath, DeleteOption deleteOption = DeleteOption.FailIfNotFound);

    bool TryMoveFile(string sourcePath, string destinationPath);

    /// <summary>
    ///     Provides logging besides normal checking
    /// </summary>
    /// <param name="directoryPath"></param>
    /// <returns></returns>
    bool CheckDirectoryExists(string directoryPath);

    bool TryDeleteDirectory(string directoryPath, DeleteOption deleteOption = DeleteOption.FailIfNotFound);

    /// <summary>
    ///     Moves a directory to <paramref name="destinationPath" />, optionally replacing an existing destination.
    ///     The destination's parent directory must already exist. The move itself is an atomic rename when both
    ///     paths are on the same volume.
    /// </summary>
    bool TryMoveDirectory(string sourcePath, string destinationPath, bool overwrite = false);
}