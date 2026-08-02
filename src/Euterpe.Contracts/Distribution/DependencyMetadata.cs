namespace Euterpe.Contracts.Distribution;

[PublicAPI]
public sealed class DependencyMetadata
{
    [JsonPropertyName("dotnet_runtime_version")]
    public string DotNetRuntimeVersion { get; set; } = string.Empty;
}
