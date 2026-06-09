namespace Euterpe.Abstractions;

public interface IFileSystemService
{
    /// <summary>
    ///     Deletes a file, throwing on failure so the caller can surface the real cause. With
    ///     <see cref="DeleteOption.IgnoreIfNotFound" /> a missing file is a no-op.
    /// </summary>
    void DeleteFile(string filePath, DeleteOption deleteOption = DeleteOption.FailIfNotFound);

    /// <summary>
    ///     Best-effort file delete: logs a warning and returns <c>false</c> on failure instead of throwing.
    ///     Use when failure is non-fatal and the caller continues regardless.
    /// </summary>
    bool TryDeleteFile(string filePath, DeleteOption deleteOption = DeleteOption.FailIfNotFound);

    /// <summary>
    ///     Moves a file to <paramref name="destinationPath" />, optionally replacing an existing destination.
    ///     The move itself is an atomic rename when both paths are on the same volume.
    /// </summary>
    bool TryMoveFile(string sourcePath, string destinationPath, bool overwrite = false);

    /// <summary>
    ///     Deletes a directory recursively, throwing on failure so the caller can surface the real cause. With
    ///     <see cref="DeleteOption.IgnoreIfNotFound" /> a missing directory is a no-op.
    /// </summary>
    void DeleteDirectory(string directoryPath, DeleteOption deleteOption = DeleteOption.FailIfNotFound);

    /// <summary>
    ///     Best-effort directory delete: logs a warning and returns <c>false</c> on failure instead of throwing.
    ///     Use when failure is non-fatal and the caller continues regardless.
    /// </summary>
    bool TryDeleteDirectory(string directoryPath, DeleteOption deleteOption = DeleteOption.FailIfNotFound);

    /// <summary>
    ///     Moves a directory to <paramref name="destinationPath" />, optionally replacing an existing destination.
    ///     The destination's parent directory must already exist. The move itself is an atomic rename when both
    ///     paths are on the same volume.
    /// </summary>
    bool TryMoveDirectory(string sourcePath, string destinationPath, bool overwrite = false);

    /// <summary>
    ///     Recursively copies a directory tree, throwing on failure so the caller can surface the real cause. The
    ///     destination is created if missing and existing files are overwritten.
    /// </summary>
    void CopyDirectory(string sourcePath, string destinationPath);
}
