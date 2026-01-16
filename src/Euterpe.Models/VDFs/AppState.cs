namespace Euterpe.Models.VDFs;

public sealed class AppState
{
    public int Appid { get; set; }
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, Dictionary<string, string>> InstalledDepots { get; set; } = new();
}