using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using static MuseDashModTools.IocContainer;

namespace MuseDashModTools.Views;

public sealed class App : Application
{
    public App() => DataContext = Resolve<AppViewModel>();

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);
        ApplyConfig();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = Resolve<MainWindow>();
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