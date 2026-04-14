namespace Euterpe.Contracts.Mods;

[PublicAPI]
public sealed class ModScreenshot
{
    public string Url { get; set; } = string.Empty;
    public string? DominantColor { get; set; }

    [JsonIgnore]
    public string FullUrl => Url.StartsWith("http") ? Url : $"https://euterpe-org.com{Url}";
}
