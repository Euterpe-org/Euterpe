#define WINDOWS

using System;
using System.IO;
using Avalonia;
using MuseDashModToolsUI.Models;
using Serilog;
using Splat;
#if DEBUG && WINDOWS
using System.Diagnostics;
using System.Runtime.InteropServices;
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
#if DEBUG && WINDOWS
        AttachToParentConsole();
#endif
        CreateLogger();
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
                if (OperatingSystem.IsWindows())
                    Process.Start("explorer.exe", "/select, " + Path.Combine("Logs", LogFileName));
                if (OperatingSystem.IsLinux())
                    Process.Start("xdg-open", "--select " + Path.Combine("Logs", LogFileName));
            }
        }
    }

    private static void RegisterDependencies() =>
        Bootstrapper.Register(Locator.CurrentMutable, Locator.Current);

    private static void CreateLogger()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
#if DEBUG && WINDOWS
            .WriteTo.Console()
#endif
            .WriteTo.File(new TextFormatter(),
                Path.Combine("Logs", LogFileName),
                rollingInterval: RollingInterval.Infinite,
                retainedFileCountLimit: 60)
            .CreateLogger();
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();

#if DEBUG && WINDOWS
    private const int AttachParentProcess = -1;

    [DllImport("kernel32.dll")]
    private static extern bool AttachConsole(int dwProcessId);

    private static void AttachToParentConsole()
    {
        AttachConsole(AttachParentProcess);
    }
#endif
}