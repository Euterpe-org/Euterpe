using System;
using System.IO;
using Avalonia;
using Serilog;
using Splat;

namespace MuseDashModToolsUI;

internal class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        CreateLogger();
        RegisterDependencies();
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    private static void RegisterDependencies() =>
        Bootstrapper.Register(Locator.CurrentMutable, Locator.Current);

    private static void CreateLogger()
    {
        var now = DateTime.Now;
        var logFileName = $"{now:yyyy-MM-dd_HH-mm-ss}.log";
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(Path.Combine("Logs", logFileName),
                rollingInterval: RollingInterval.Infinite,
                retainedFileCountLimit: 60)
            .CreateLogger();
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}