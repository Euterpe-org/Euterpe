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
    ///     Copies a file to <paramref name="destinationPath" />, creating the destination directory if missing.
    ///     Best-effort: logs a warning and returns <c>false</c> on failure instead of throwing.
    /// </summary>
    bool TryCopyFile(string sourcePath, string destinationPath, bool overwrite = false);

    /// <summary>
    ///     Writes <paramref name="bytes" /> into a temporary sibling file, then atomically renames it onto
    ///     <paramref name="filePath" /> so a crash mid-write cannot corrupt an existing file. Best-effort: logs a
    ///     warning, removes the temporary file, and returns <c>false</c> on failure, leaving any existing file untouched.
    /// </summary>
    Task<bool> TryWriteFileAtomicAsync(string filePath, ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken = default);

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

    /// <summary>
    ///     Returns the top-level files of <paramref name="directory" /> as a case-insensitive name-to-size map,
    ///     or an empty map when the directory does not exist.
    /// </summary>
    IReadOnlyDictionary<string, long> GetFileSizes(string directory);
}
