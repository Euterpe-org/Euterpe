using System;
using System.Diagnostics;
using System.IO;
using Autofac;
using Avalonia;
using Avalonia.Media;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Models;
using Serilog;

namespace MuseDashModToolsUI;

internal static class Program
{
    private static readonly string LogFileName = $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log";
    private static IContainer? _container;

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        CreateLogger();
        _container = RegisterDependencies();
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

    private static IContainer RegisterDependencies() => Bootstrapper.Register();

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

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .With(new FontManagerOptions
            {
                DefaultFamilyName = _container?.Resolve<ISettingService>().Settings.FontName,
                FontFallbacks = new[]
                {
                    new FontFallback { FontFamily = new FontFamily(_container?.Resolve<ISettingService>().Settings.FontName!) }
                }
            });
}