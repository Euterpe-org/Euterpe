using Autofac.Extensions.DependencyInjection;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using HotAvalonia;
using Microsoft.Extensions.DependencyInjection;

namespace MuseDashModTools;

public sealed class App : Application
{
    private static readonly string LogFileName = $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log";
    public static readonly IContainer Container = ConfigureServices();

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
            desktop.MainWindow = Container.Resolve<WizardWindow>();

            Observable.FromEventHandler<ControlledApplicationLifetimeExitEventArgs>(
                    handler => desktop.Exit += handler,
                    handler => desktop.Exit -= handler)
                .OnErrorResumeAsFailure()
                .SubscribeAwait((_, _) => new ValueTask(OnExitAsync()),
                    result => Container.Resolve<ILogger<App>>().ZLogError(result.Exception, $"Exception when saving Setting"),
                    configureAwait: false);
        }

        this.ObservePropertyChanged(x => x.ActualThemeVariant)
            .Subscribe(theme => Container.Resolve<Config>().Theme = AvaloniaResources.ThemeVariants[theme]);

        base.OnFrameworkInitializationCompleted();
    }

    private static Task OnExitAsync() => Container.Resolve<ISettingService>().SaveAsync();

#if RELEASE
    private static void LogException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Container.Resolve<ILogger<App>>().ZLogError(e.Exception, $"Unhandled exception");
        Container.Resolve<IPlatformService>().RevealFile(Path.Combine("Logs", LogFileName));
        Container.Resolve<IPlatformService>().OpenUriAsync("https://github.com/MDModsDev/MuseDashModTools/issues/new/choose");
    }
#endif
}