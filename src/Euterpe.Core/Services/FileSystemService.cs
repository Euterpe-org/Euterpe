namespace Euterpe.Core;

internal sealed class FileSystemService : IFileSystemService
{
    private const int MaxAvailablePathAttempts = 8;

    #region Injections

    public required ILogger<FileSystemService> Logger { get; init; }

    #endregion Injections

    public void DeleteFile(string filePath, DeleteOption deleteOption = DeleteOption.FailIfNotFound)
    {
        if (deleteOption is DeleteOption.IgnoreIfNotFound && !File.Exists(filePath))
        {
            return;
        }

        File.Delete(filePath);
    }

    public bool TryDeleteFile(string filePath, DeleteOption deleteOption = DeleteOption.FailIfNotFound)
    {
        try
        {
            DeleteFile(filePath, deleteOption);
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
            if (string.Equals(Path.GetFullPath(sourcePath), Path.GetFullPath(destinationPath), StringComparison.OrdinalIgnoreCase))
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
            TryDeleteFile(tempPath, DeleteOption.IgnoreIfNotFound);
            return false;
        }
    }

    public DateTime? GetFileLastWriteTimeUtc(string filePath) =>
        File.Exists(filePath) ? File.GetLastWriteTimeUtc(filePath) : null;

    public Stream? TryOpenReadFile(string filePath)
    {
        try
        {
            return File.OpenRead(filePath);
        }
        catch (Exception ex)
        {
            Logger.ZLogWarning(ex, $"Failed to open file {filePath}");
            return null;
        }
    }

    public void DeleteDirectory(string directoryPath, DeleteOption deleteOption = DeleteOption.FailIfNotFound)
    {
        if (deleteOption is DeleteOption.IgnoreIfNotFound && !Directory.Exists(directoryPath))
        {
            return;
        }

        Directory.Delete(directoryPath, true);
    }

    public bool TryDeleteDirectory(string directoryPath, DeleteOption deleteOption = DeleteOption.FailIfNotFound)
    {
        try
        {
            DeleteDirectory(directoryPath, deleteOption);
            return true;
        }
        catch (Exception ex)
        {
            Logger.ZLogWarning(ex, $"Failed to delete directory {directoryPath}");
            return false;
        }
    }

    public bool TryMoveDirectory(string sourcePath, string destinationPath, bool overwrite = false)
    {
        try
        {
            if (overwrite && Directory.Exists(destinationPath))
            {
                Directory.Delete(destinationPath, true);
            }

            Directory.Move(sourcePath, destinationPath);
            return true;
        }
        catch (Exception ex)
        {
            Logger.ZLogWarning(ex, $"Failed to move directory from {sourcePath} to {destinationPath}");
            return false;
        }
    }

    public bool TryMoveDirectoryToAvailablePath(string sourcePath, string desiredPath, out string finalPath)
    {
        var parent = Path.GetDirectoryName(desiredPath);
        var candidate = desiredPath;

        for (var attempt = 0; attempt < MaxAvailablePathAttempts; attempt++)
        {
            try
            {
                if (!parent.IsNullOrEmpty())
                {
                    Directory.CreateDirectory(parent);
                }

                Directory.Move(sourcePath, candidate);
                finalPath = candidate;
                return true;
            }
            catch (IOException) when (Directory.Exists(candidate))
            {
                candidate = $"{desiredPath}-{Guid.NewGuid().ToString("N")[..8]}";
            }
            catch (Exception ex)
            {
                Logger.ZLogWarning(ex, $"Failed to move directory from {sourcePath} to {candidate}");
                finalPath = string.Empty;
                return false;
            }
        }

        Logger.ZLogWarning($"Exhausted available-path attempts moving directory from {sourcePath} to {desiredPath}");
        finalPath = string.Empty;
        return false;
    }

    public void CopyDirectory(string sourcePath, string destinationPath)
    {
        Directory.CreateDirectory(destinationPath);

        foreach (var file in Directory.EnumerateFiles(sourcePath))
        {
            File.Copy(file, Path.Combine(destinationPath, Path.GetFileName(file)), true);
        }

        foreach (var directory in Directory.EnumerateDirectories(sourcePath))
        {
            CopyDirectory(directory, Path.Combine(destinationPath, Path.GetFileName(directory)));
        }
    }

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
