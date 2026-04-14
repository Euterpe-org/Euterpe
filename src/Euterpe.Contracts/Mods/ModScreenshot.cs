namespace Euterpe.Contracts.Mods;

[PublicAPI]
public sealed class ModScreenshot
{
    public string Url { get; set; } = string.Empty;
    public string? DominantColor { get; set; }
}
