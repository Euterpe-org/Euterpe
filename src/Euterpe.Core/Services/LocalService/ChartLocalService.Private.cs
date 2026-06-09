using Euterpe.Models.Charts.CustomAlbums;

namespace Euterpe.Core;

internal sealed partial class ChartLocalService
{
    private CustomAlbumSource[] ResolveSources(IEnumerable<string> paths) =>
        paths
            .Select(CreateCustomAlbumSource)
            .GroupBy(source => source.Name, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.MaxBy(SourceTimestamp)!)
            .ToArray();

    private static DateTime SourceTimestamp(CustomAlbumSource source) =>
        source.IsFolder
            ? Directory.EnumerateFiles(source.Path).Max(File.GetLastWriteTimeUtc)
            : File.GetLastWriteTimeUtc(source.Path);
}
