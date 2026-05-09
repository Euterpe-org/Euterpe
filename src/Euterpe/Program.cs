using static Euterpe.Bootstrapper;
using static Euterpe.CrashHandler;
using static Euterpe.IocContainer;

namespace Euterpe;

internal static class Program
{
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
        ConfigureContainer();
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
            .UseR3(ex => ReportException(ex))
            .HandleUIThreadException(ex => ex.Handled = ReportException(ex.Exception));
}