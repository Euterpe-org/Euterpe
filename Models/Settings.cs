namespace MuseDashModToolsUI.Models;

public class Settings
{
    public string MuseDashFolder { get; set; }

    public Settings()
    {
        
    }

    public Settings(string museDashFolder)
    {
        MuseDashFolder = museDashFolder;
    }
}