namespace MuseDashModTools.Models;

public sealed class AppState
{
    public int Appid { get; set; }
    public string? Name { get; set; }
    public Dictionary<long, Dictionary<string, long>> InstalledDepots { get; set; } = new();
}