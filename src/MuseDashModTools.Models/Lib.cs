namespace MuseDashModTools.Models;

[PublicAPI]
public sealed class Lib
{
    public string FileName { get; set; } = string.Empty;
    public string SHA256 { get; set; } = string.Empty;
}