namespace Euterpe.Abstractions;

public partial interface IFileSystemService
{
    /// <summary>
    ///     Best-effort file deletion: logs a warning and returns <c>false</c> on failure instead of throwing.
    ///     A missing file is treated as successfully deleted.
    /// </summary>
    bool TryDeleteFile(string filePath);

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
    ///     Returns the file's last write time in UTC, or <c>null</c> when the file does not exist.
    /// </summary>
    DateTime? GetFileLastWriteTimeUtc(string filePath);

    /// <summary>
    ///     Returns the top-level files of <paramref name="directory" /> as a case-insensitive name-to-size map,
    ///     or an empty map when the directory does not exist.
    /// </summary>
    IReadOnlyDictionary<string, long> GetFileSizes(string directory);
}
