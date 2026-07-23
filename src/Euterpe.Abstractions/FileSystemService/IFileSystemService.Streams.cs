namespace Euterpe.Abstractions;

public partial interface IFileSystemService
{
    /// <summary>
    ///     Opens a file for reading, or returns <c>null</c> when it cannot be opened. The caller owns the returned stream.
    /// </summary>
    Stream? TryOpenReadFile(string filePath);
}
