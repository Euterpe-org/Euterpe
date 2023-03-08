namespace MuseDashModToolsUI.Models;

public class Settings
{
    public string ModsFolder { get; set; }

    public Settings()
    {
        
    }
    public Settings(string modsFolder)
    {
        ModsFolder = modsFolder;
    }
}