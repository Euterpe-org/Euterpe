using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json.Nodes;
using Avalonia;
using Avalonia.Media;
using MuseDashModToolsUI.Models;
using Serilog;

namespace MuseDashModToolsUI;

internal static class Program
{
    private static readonly string LogFileName = $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log";
    private static string? _fontName;

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        CreateLogger();
        RegisterDependencies();
        ReadSavedFontName();
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
                rollingInterval: RollingInterval.Infinite,
                retainedFileCountLimit: 60)
            .CreateLogger();
    }

    private static void ReadSavedFontName()
    {
        var text = File.ReadAllText("Settings.json");
        var settings = JsonNode.Parse(text);
        _fontName = settings?["FontName"]?.ToString();
        if (string.IsNullOrEmpty(_fontName))
            _fontName = "Segoe UI";
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .With(new FontManagerOptions
            {
                FontFallbacks = new[]
                {
                    new FontFallback { FontFamily = new FontFamily(_fontName!) }
                }
            });
}