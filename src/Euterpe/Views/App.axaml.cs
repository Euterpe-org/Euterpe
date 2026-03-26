using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using static Euterpe.IocContainer;

namespace Euterpe.Views;

public sealed class App : Application
{
    public App() => DataContext = Resolve<AppViewModel>();

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        ApplyConfig();

        var deepLinkService = Resolve<DeepLinkService>();
        deepLinkService.SetupAsync().SafeFireAndForget();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = Resolve<MainSplashWindow>();
            deepLinkService.HandleStartupArgs(desktop.Args!);
            HandleDesktopExit(desktop);
        }

        this.ObservePropertyChanged(x => x.ActualThemeVariant)
            .Subscribe(theme => Resolve<Config>().Theme = AvaloniaResources.ThemeVariants[theme]);

        base.OnFrameworkInitializationCompleted();
    }

    private void ApplyConfig()
    {
        var config = Resolve<Config>();
        RequestedThemeVariant = AvaloniaResources.ThemeVariants[config.Theme];
        Resolve<LocalizationService>().SetLanguage(config.LanguageCode);
    }

    private static void HandleDesktopExit(IClassicDesktopStyleApplicationLifetime desktop)
    {
        Observable.FromEventHandler<ControlledApplicationLifetimeExitEventArgs>(
                handler => desktop.Exit += handler,
                handler => desktop.Exit -= handler)
            .Take(1)
            .SubscribeAwait((_, _) => new ValueTask(Resolve<ISettingService>().SaveAsync()),
                _ => Resolve<ILogger<App>>().ZLogInformation($"Setting saved successfully"),
                configureAwait: false);
    }
}