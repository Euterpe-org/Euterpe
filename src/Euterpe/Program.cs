using static Euterpe.Bootstrapper;
using static Euterpe.IocContainer;

namespace Euterpe;

internal static class Program
{
    private static readonly string LogFileName = $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log";
    private static readonly string LogFilePath = Path.Combine(AppLogsFolder, LogFileName);

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        using var mutex = new Mutex(true, AppName, out var createdNew);
        if (!createdNew)
        {
            if (args is not [])
            {
                SendArgsToPrimaryInstance(args);
            }

            return;
        }

        Directory.CreateDirectory(AppDataFolder);
        CleanupLogFiles();
        CleanupBackupFiles();
        ConfigureContainer(LogFilePath);
        StartDeepLinkPipeServer();
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        StopDeepLinkPipeServer();
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
#if DEBUG
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
        Resolve<ILogger<App>>().ZLogCritical(ex, $"Unhandled exception");
#if PUBLISH
        Resolve<IPlatformService>().RevealFile(Path.Combine("Logs", LogFileName));
        Resolve<IPlatformService>().OpenUriAsync("https://github.com/Euterpe-org/Euterpe/issues/new/choose");
#endif
    }
}