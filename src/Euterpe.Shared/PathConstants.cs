namespace Euterpe.Shared;

public static class PathConstants
{
    public static readonly string AppDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppName);
    public static readonly string AppLogsFolder = Path.Combine(AppContext.BaseDirectory, "Logs");
    public static readonly string LogFilePath = Path.Combine(AppLogsFolder, $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log");
}