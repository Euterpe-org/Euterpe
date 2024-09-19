using Avalonia.Dialogs;
#if !DEBUG
using System.Diagnostics;
#endif

namespace MuseDashModToolsUI;

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
#if !DEBUG
            if (File.Exists(Path.Combine("Logs", LogFileName)))
            {
                if (OperatingSystem.IsWindows())
                    Process.Start("explorer.exe", "/select, " + Path.Combine("Logs", LogFileName));
                else if (OperatingSystem.IsLinux())
                    Process.Start("xdg-open", Path.Combine("Logs", LogFileName));
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://github.com/MDModsDev/MuseDashModToolsUI/issues/new/choose",
                    UseShellExecute = true
                });
            }
#endif
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
        var logFiles = Directory.GetFiles("Logs", "*.log").OrderDescending().Skip(60).ToArray();
        if (logFiles is [])
        {
            return;
        }

        Parallel.ForEach(logFiles, (logFile, _) => File.Delete(logFile));
    }

    // Avalonia configuration, don't remove; also used by visual designer.
#pragma warning disable CA1416
    private static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
        .UseManagedSystemDialogs()
        .UsePlatformDetect()
        .WithInterFont()
        .LogToTrace();
}