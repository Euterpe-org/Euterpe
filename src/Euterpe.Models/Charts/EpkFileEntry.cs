namespace Euterpe.Models.Charts;

public sealed record EpkFileEntry(string Name, long Size)
{
    public string SizeDisplay => Size.ToReadableSize();
}
