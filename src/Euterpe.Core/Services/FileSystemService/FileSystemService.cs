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
            Logger.ZLogWarning(ex, $"Failed to delete file {filePath}");
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
            Logger.ZLogWarning(ex, $"Failed to move file from {sourcePath} to {destinationPath}");
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
            Logger.ZLogWarning(ex, $"Failed to copy file from {sourcePath} to {destinationPath}");
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
            Logger.ZLogWarning(ex, $"Failed to write file {filePath}");
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
