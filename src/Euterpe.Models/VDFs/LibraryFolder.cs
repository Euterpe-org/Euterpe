namespace Euterpe.Models.VDFs;

[PublicAPI]
public sealed class LibraryFolder
{
    public string Path { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string ContentId { get; set; } = string.Empty;
    public string TotalSize { get; set; } = string.Empty;
    public Dictionary<string, string> Apps { get; set; } = new();
}
