using Euterpe.Core.Proxies;
using WindowNotificationManager = Ursa.Controls.WindowNotificationManager;

namespace Euterpe.Extensions;

public static partial class ServiceExtensions
{
    public static void RegisterInternalServices(this ContainerBuilder builder)
    {
        // Self Services
        builder.RegisterType<DeepLinkService>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<DialogService>().As<IDialogService>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<GameSwitcher>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<NavigationService>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<LocalizationService>().PropertiesAutowired().SingleInstance();

        // Auto Activate Services
        builder.RegisterType<LiveLogService>().PropertiesAutowired().SingleInstance().AutoActivate();

        // TopLevel
        builder.RegisterType<TopLevelProxy>().SingleInstance();

        // Notification
        builder.Register<WindowNotificationManager>(ctx => ctx.Resolve<MainWindow>().Notifier).SingleInstance();
    }

    public static void RegisterWindowsAndViewModels(this ContainerBuilder builder)
    {
        builder.RegisterType<MainWindowViewModel>().PropertiesAutowired().SingleInstance();
        builder.Register(static ctx => new MainWindow { DataContext = ctx.Resolve<MainWindowViewModel>() }).SingleInstance();

        builder.RegisterType<MainSplashWindowViewModel>().PropertiesAutowired().SingleInstance();
        builder.Register(static ctx => new MainSplashWindow { DataContext = ctx.Resolve<MainSplashWindowViewModel>() }).SingleInstance();

        builder.RegisterType<CrashWindowViewModel>().PropertiesAutowired().SingleInstance();
        builder.Register(static ctx => new CrashWindow { DataContext = ctx.Resolve<CrashWindowViewModel>() }).InstancePerDependency();
    }
}