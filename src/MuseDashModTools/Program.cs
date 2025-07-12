using static MuseDashModTools.IocContainer;

namespace MuseDashModTools;

internal static class Program
{
    private static readonly string LogFileName = $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log";

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        // Prevent multiple launch
        using var mutex = new Mutex(true, AppName);
        if (!mutex.WaitOne(TimeSpan.Zero, true))
        {
            return;
        }

        DeleteUnusedLogFile();
        ConfigureContainer(LogFileName);
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    private static void DeleteUnusedLogFile()
    {
        const string logFolderName = "Logs";

        if (!Directory.Exists(logFolderName))
        {
            return;
        }

        var logFiles = Directory.GetFiles(logFolderName, "*.log").OrderDescending().Skip(30).ToArray();
        if (logFiles is [])
        {
            return;
        }

        Parallel.ForEach(logFiles, (logFile, _) => File.Delete(logFile));
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .WithInterFont()
        .LogToTrace()
#if (DEBUG && ACCELERATE)
        .WithDeveloperTools()
#endif
        .UseR3(ReportException)
        .HandleUIThreadException(ex =>
        {
            ReportException(ex.Exception);
            ex.Handled = Resolve<Config>().IgnoreException;
        });

    private static void ReportException(Exception ex)
    {
        Resolve<ILogger<App>>().ZLogError(ex, $"Unhandled exception");
#if PUBLISH
        Resolve<IPlatformService>().RevealFile(Path.Combine("Logs", LogFileName));
        Resolve<IPlatformService>().OpenUriAsync("https://github.com/MDModsDev/MuseDashModTools/issues/new/choose");
#endif
    }
}