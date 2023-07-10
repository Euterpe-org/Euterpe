using System.Collections.Generic;

namespace MuseDashModToolsUI.Models;

public class AppState
{
    public int Appid { get; set; }
    public string? Name { get; set; }
    public Dictionary<long, Dictionary<string, long>> InstalledDepots { get; set; } = new();
}