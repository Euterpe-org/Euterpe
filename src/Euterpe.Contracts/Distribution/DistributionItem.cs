namespace Euterpe.Contracts.Distribution;

public abstract class DistributionItem<TMetadata> where TMetadata : new()
{
    public string Slug { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public Dictionary<string, DistributionVersion<TMetadata>> Versions { get; set; } = [];
}