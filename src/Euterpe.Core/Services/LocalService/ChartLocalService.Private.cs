using Euterpe.Models.Charts.CustomAlbums;

namespace Euterpe.Core;

internal sealed partial class ChartLocalService
{
    private static CustomAlbumSource[] ResolveSources(IEnumerable<string> paths) =>
        paths
            .Select(ToSource)
            .GroupBy(source => source.Name, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.MaxBy(SourceTimestamp)!)
            .ToArray();

    private static CustomAlbumSource ToSource(string path)
    {
        var isFolder = Directory.Exists(path);
        return new CustomAlbumSource(
            path,
            isFolder ? Path.GetFileName(path) : Path.GetFileNameWithoutExtension(path),
            isFolder);
    }

    private static DateTime SourceTimestamp(CustomAlbumSource source) =>
        source.IsFolder
            ? Directory.EnumerateFiles(source.Path).Max(File.GetLastWriteTimeUtc)
            : File.GetLastWriteTimeUtc(source.Path);
}
