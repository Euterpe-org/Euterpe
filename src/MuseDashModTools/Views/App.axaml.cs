using Autofac.Extensions.DependencyInjection;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;

namespace MuseDashModTools.Views;

public sealed class App : Application
{
    private static readonly string LogFileName = $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log";
    public static readonly IContainer Container = ConfigureServices();

    public App() => DataContext = Container.Resolve<AppViewModel>();

    private static IContainer ConfigureServices()
    {
        var services = new ServiceCollection();
        services.RegisterLogger(LogFileName);

        var builder = new ContainerBuilder();
        builder.RegisterInstances();
        builder.RegisterCoreServices();
        builder.RegisterInternalServices();
        builder.RegisterLazyProxies();
        builder.RegisterViewAndViewModels();

        builder.Populate(services);
        return builder.Build();
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        Dispatcher.UIThread.UnhandledException += (_, e) => Container.Resolve<ILogger<App>>().ZLogError(e.Exception, $"Unhandled exception");
        // HandleUIThreadException();

        ApplyConfig();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = Container.Resolve<MainWindow>();
            HandleDesktopExit(desktop);
        }

        this.ObservePropertyChanged(x => x.ActualThemeVariant)
            .Subscribe(theme => Container.Resolve<Config>().Theme = AvaloniaResources.ThemeVariants[theme]);

        base.OnFrameworkInitializationCompleted();
    }

    private void ApplyConfig()
    {
        var config = Container.Resolve<Config>();
        RequestedThemeVariant = AvaloniaResources.ThemeVariants[config.Theme];
        Container.Resolve<LocalizationService>().SetLanguage(config.LanguageCode);
    }

    private static void HandleDesktopExit(IClassicDesktopStyleApplicationLifetime desktop)
    {
        Observable.FromEventHandler<ControlledApplicationLifetimeExitEventArgs>(
                handler => desktop.Exit += handler,
                handler => desktop.Exit -= handler)
            .Take(1)
            .OnErrorResumeAsFailure()
            .SubscribeAwait((_, _) => new ValueTask(Container.Resolve<ISettingService>().SaveAsync()),
                result => Container.Resolve<ILogger<App>>().ZLogError(result.Exception, $"{result.IsFailure}"),
                configureAwait: false);
    }


    private static void HandleUIThreadException()
    {
        Observable.FromEvent<DispatcherUnhandledExceptionEventHandler, DispatcherUnhandledExceptionEventArgs>(
                handler => (sender, args) => handler(args),
                handler => Dispatcher.UIThread.UnhandledException += handler,
                handler => Dispatcher.UIThread.UnhandledException -= handler)
            .Subscribe(e =>
            {
                Container.Resolve<ILogger<App>>().ZLogError(e.Exception, $"Unhandled exception");
                Container.Resolve<IPlatformService>().RevealFile(Path.Combine("Logs", LogFileName));
                Container.Resolve<IPlatformService>().OpenUriAsync("https://github.com/MDModsDev/MuseDashModTools/issues/new/choose");
            });
    }
}