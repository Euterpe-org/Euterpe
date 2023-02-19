namespace MuseDashModToolsUI.Models;

public class LocalModInfo
{
    public string? Name { get; set; }
    public string? Version { get; set; }
    public string? SHA256 { get; set; }
    public string? FileName { get; set; }
    public bool Disabled { get; set; }
    public string? Description { get; set; }
    public string? Author { get; set; }
    public string? HomePage { get; set; }

    public string FileNameExtended(bool reverse = false)
    {
        return FileName + ((reverse ? !Disabled : Disabled) ? ".disabled" : "");

    }
}