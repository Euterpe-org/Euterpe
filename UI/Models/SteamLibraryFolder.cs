using System.Collections.Generic;

namespace MuseDashModToolsUI.Models;

public class SteamLibraryFolder
{
    public string? Path { get; set; }
    public Dictionary<int, string> Apps { get; set; } = new();
}