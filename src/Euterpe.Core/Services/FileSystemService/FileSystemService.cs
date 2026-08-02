namespace Euterpe.Core;

internal sealed partial class FileSystemService : IFileSystemService
{
    #region Injections

    public required ILogger<FileSystemService> Logger { get; init; }

    #endregion Injections

    public bool TryDeleteFile(string filePath)
    {
        try
        {
            File.Delete(filePath);
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to delete file {FilePath}", filePath);
            return false;
        }
    }

    public bool TryMoveFile(string sourcePath, string destinationPath, bool overwrite = false)
    {
        try
        {
            File.Move(sourcePath, destinationPath, overwrite);
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to move file from {SourcePath} to {DestinationPath}", sourcePath, destinationPath);
            return false;
        }
    }

    public bool TryCopyFile(string sourcePath, string destinationPath, bool overwrite = false)
    {
        try
        {
            if (string.Equals(Path.GetFullPath(sourcePath), Path.GetFullPath(destinationPath), StringComparison.Ordinal))
            {
                return true;
            }

            var directory = Path.GetDirectoryName(destinationPath);
            if (!directory.IsNullOrEmpty())
            {
                Directory.CreateDirectory(directory);
            }

            File.Copy(sourcePath, destinationPath, overwrite);
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to copy file from {SourcePath} to {DestinationPath}", sourcePath, destinationPath);
            return false;
        }
    }

    public async Task<bool> TryWriteFileAtomicAsync(string filePath, ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken = default)
    {
        var tempPath = filePath + ".tmp";
        try
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!directory.IsNullOrEmpty())
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllBytesAsync(tempPath, bytes, cancellationToken).ConfigureAwait(false);
            File.Move(tempPath, filePath, true);
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to write file {FilePath}", filePath);
            TryDeleteFile(tempPath);
            return false;
        }
    }

    public DateTime? GetFileLastWriteTimeUtc(string filePath) =>
        File.Exists(filePath) ? File.GetLastWriteTimeUtc(filePath) : null;

    public IReadOnlyDictionary<string, long> GetFileSizes(string directory)
    {
        var sizes = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        if (!Directory.Exists(directory))
        {
            return sizes;
        }

        foreach (var file in new DirectoryInfo(directory).EnumerateFiles())
        {
            sizes[file.Name] = file.Length;
        }

        return sizes;
    }
}
