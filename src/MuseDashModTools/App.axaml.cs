using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using HotAvalonia;
using MuseDashModTools.Core.Extensions;

namespace MuseDashModTools;

public sealed class App : Application
{
    private static readonly string LogFileName = $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log";
    public readonly IContainer Container = ConfigureServices();
    public static new App? Current => Application.Current as App;

    private static IContainer ConfigureServices()
    {
        var builder = new ContainerBuilder();

        builder.RegisterLogger(LogFileName);
        builder.RegisterCoreServices();
        builder.RegisterInstances();
        builder.RegisterInternalServices();
        builder.RegisterViewAndViewModels();

        return builder.Build();
    }

    public override void Initialize()
    {
        this.EnableHotReload();
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
#if RELEASE
        Dispatcher.UIThread.UnhandledException += LogException;
#endif
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            BindingPlugins.DataValidators.RemoveAt(0);
            desktop.MainWindow = Container.Resolve<MainWindow>();
        }

        base.OnFrameworkInitializationCompleted();
    }

#if RELEASE
    private static void LogException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Logger.Fatal(e.Exception, "Unhandled exception");
     if (OperatingSystem.IsWindows())
        {
            Process.Start("explorer.exe", "/select, " + Path.Combine("Logs", LogFileName));
        }
        else if (OperatingSystem.IsLinux())
        {
            Process.Start("xdg-open", Path.Combine("Logs", LogFileName));
        }
     Process.Start(new ProcessStartInfo
        {
            FileName = "https://github.com/MDModsDev/MuseDashModTools/issues/new/choose",
            UseShellExecute = true
        });
    }
#endif
}