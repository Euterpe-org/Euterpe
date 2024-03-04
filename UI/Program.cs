#if !DEBUG
using System.Diagnostics;
#endif
namespace MuseDashModToolsUI;

internal static class Program
{
    [UsedImplicitly]
    private const string IssuePage = "https://github.com/MDModsDev/MuseDashModToolsUI/issues/new/choose";

    private static readonly string LogFileName = $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log";

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        // Prevent multiple launch
        using var mutex = new Mutex(true, "MuseDashModTools");
        if (!mutex.WaitOne(TimeSpan.Zero, true))
        {
            return;
        }

        CreateLogger();
        DeleteUnusedLogFile();
        RegisterDependencies();
        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            Log.Logger.Fatal(ex, "Unhandled exception");
            if (File.Exists(Path.Combine("Logs", LogFileName)))
            {
#if !DEBUG
                if (OperatingSystem.IsWindows())
                    Process.Start("explorer.exe", "/select, " + Path.Combine("Logs", LogFileName));
                if (OperatingSystem.IsLinux())
                    Process.Start("xdg-open", Path.Combine("Logs", LogFileName));
                Process.Start(new ProcessStartInfo
                {
                    FileName = IssuePage,
                    UseShellExecute = true
                });
#endif
            }
        }
    }

    private static void RegisterDependencies() => Bootstrapper.Register();

    private static void CreateLogger()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
#if DEBUG
            .WriteTo.Console()
#endif
            .WriteTo.File(new LogFileFormatter(),
                Path.Combine("Logs", LogFileName),
                rollingInterval: RollingInterval.Infinite)
            .CreateLogger();
    }

    private static void DeleteUnusedLogFile()
    {
        var logs = Directory.GetFiles("Logs", "*.log").OrderDescending().Skip(60);
        Parallel.ForEachAsync(logs, async (log, c) => await Task.Run(() => File.Delete(log), c));
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}