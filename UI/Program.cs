using System;
using System.Diagnostics;
using System.IO;
using Avalonia;
using Serilog;
using Splat;

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
        CreateLogger();
        RegisterDependencies();
        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            Log.Logger.Fatal("Unhandled exception: {Exception}", ex.ToString());
            if (File.Exists(Path.Combine("Logs", LogFileName)))
                Process.Start("explorer.exe", "/select," + Path.Combine("Logs", LogFileName));
        }
    }

    private static void RegisterDependencies() =>
        Bootstrapper.Register(Locator.CurrentMutable, Locator.Current);

    private static void CreateLogger()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(Path.Combine("Logs", LogFileName),
                outputTemplate: "[{Timestamp:HH:mm:ss.fff zzz}] [{Level}]{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}",
                rollingInterval: RollingInterval.Infinite,
                retainedFileCountLimit: 60)
            .CreateLogger();
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}