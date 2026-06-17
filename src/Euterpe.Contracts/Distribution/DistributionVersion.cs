namespace Euterpe.Contracts.Distribution;

[PublicAPI]
public sealed class DistributionVersion<TMetadata> where TMetadata : new()
{
    [JsonPropertyName("sha256")]
    public string SHA256 { get; set; } = string.Empty;

    public long FileSize { get; set; }
    public TMetadata Metadata { get; set; } = new();
    public string DownloadUrl { get; set; } = string.Empty;
}
