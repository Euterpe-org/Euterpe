namespace Euterpe.Abstractions;

public partial interface IFileSystemService
{
    /// <summary>
    ///     Deletes a directory recursively, throwing on failure so the caller can surface the real cause. With
    ///     <see cref="DeleteOption.IgnoreIfNotFound" /> a missing directory is a no-op.
    /// </summary>
    void DeleteDirectory(string directoryPath, DeleteOption deleteOption = DeleteOption.FailIfNotFound);

    /// <summary>
    ///     Best-effort directory deletion: logs a warning and returns <c>false</c> on failure instead of throwing.
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
    ///     Moves a directory to <paramref name="desiredPath" />, or to a randomly suffixed sibling
    ///     (<c>desiredPath-xxxxxxxx</c>) when that path is already taken, reporting the path actually used in
    ///     <paramref name="finalPath" />. The move is the atomic claim, so the operation stays correct when several
    ///     callers target the same destination concurrently. The destination's parent is created if missing.
    ///     Best-effort: logs a warning and returns <c>false</c> on failure.
    /// </summary>
    bool TryMoveDirectoryToAvailablePath(string sourcePath, string desiredPath, out string finalPath);

    /// <summary>
    ///     Recursively copies a directory tree, throwing on failure so the caller can surface the real cause. The
    ///     destination is created if missing and existing files are overwritten.
    /// </summary>
    void CopyDirectory(string sourcePath, string destinationPath);
}
