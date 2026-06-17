using static System.IO.Path;

namespace Euterpe.Models.Charts.CustomAlbums;

public sealed record CustomAlbumSource(string Path, bool IsFolder)
{
    public string Name { get; } = IsFolder ? GetFileName(Path) : GetFileNameWithoutExtension(Path);
}
