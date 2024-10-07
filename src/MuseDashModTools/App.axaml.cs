using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using HotAvalonia;
using MuseDashModTools.Core.Extensions;

namespace MuseDashModTools;

public sealed class App : Application
{
    public IContainer Container { get; } = ConfigureServices();
    public static new App? Current => Application.Current as App;

    private static IContainer ConfigureServices()
    {
        var builder = new ContainerBuilder();
        builder.RegisterLogger();
        builder.RegisterCoreServices();
        builder.RegisterInstances();
        builder.RegisterServices();

        return builder.Build();
    }

    public override void Initialize()
    {
        this.EnableHotReload();
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            BindingPlugins.DataValidators.RemoveAt(0);
            desktop.MainWindow = Container.Resolve<MainWindow>();
        }

        base.OnFrameworkInitializationCompleted();
    }
}