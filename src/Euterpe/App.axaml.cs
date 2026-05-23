using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using static Euterpe.IocContainer;

namespace Euterpe;

public sealed class App : Application
{
    public App() => DataContext = Resolve<AppViewModel>();

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Initialize Window/UserControl with async initializers when they are loaded
        Control.LoadedEvent.AddClassHandler<Window>(OnControlLoaded);
        Control.LoadedEvent.AddClassHandler<UserControl>(OnControlLoaded);
        Resolve<AppViewModel>().InitializeAsync().SafeFireAndForget();

        ApplyConfig();

        var deepLinkService = Resolve<DeepLinkService>();
        deepLinkService.SetupAsync().SafeFireAndForget(ex => Resolve<ILogger<App>>().ZLogError(ex, $"Failed to register deep-link OS handler"));

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = Resolve<MainSplashWindow>();
            deepLinkService.HandleStartupArgs(desktop.Args!);
        }

        if (ApplicationLifetime is IControlledApplicationLifetime controlledLifetime)
        {
            HandleExit(controlledLifetime);
        }

        this.ObservePropertyChanged(x => x.ActualThemeVariant)
            .Subscribe(theme => Resolve<Config>().Theme = AvaloniaResources.ThemeVariants[theme]);

        base.OnFrameworkInitializationCompleted();
    }

    private static void OnControlLoaded(ContentControl control, RoutedEventArgs _)
    {
        if (control.DataContext is not IAsyncInitializable initializable)
        {
            return;
        }

        initializable.InitializeAsync().SafeFireAndForget(ex => Resolve<ILogger<App>>().ZLogError(ex, $"Async initializer for {control.GetType().Name} failed"));
    }

    private void ApplyConfig()
    {
        var config = Resolve<Config>();
        RequestedThemeVariant = AvaloniaResources.ThemeVariants[config.Theme];
        Resolve<LocalizationService>().SetLanguage(config.LanguageCode);
    }

    private static void HandleExit(IControlledApplicationLifetime controlledLifetime)
    {
        controlledLifetime.Exit += (_, _) =>
        {
            try
            {
                Resolve<IAppSettingService>().Save();
            }
            catch (Exception ex)
            {
                Resolve<ILogger<App>>().ZLogError(ex, $"Failed to save settings on exit");
            }
        };
    }
}