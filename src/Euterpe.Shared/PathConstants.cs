namespace Euterpe.Shared;

public static class PathConstants
{
    public static readonly string AppDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppName);
    public static readonly string LocalAppDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppName);
    public static readonly string AppLogsFolder = Path.Combine(LocalAppDataFolder, "Logs");
    public static readonly string LogFilePath = Path.Combine(AppLogsFolder, $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log");
}
