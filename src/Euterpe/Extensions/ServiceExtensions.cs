using Euterpe.Core.Proxies;
using WindowNotificationManager = Ursa.Controls.WindowNotificationManager;

namespace Euterpe.Extensions;

public static partial class ServiceExtensions
{
    extension(ContainerBuilder builder)
    {
        public void RegisterInternalServices()
        {
            // Self Services
            builder.RegisterType<DeepLinkService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<GameSwitcher>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<LocalizationService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<NavigationService>().PropertiesAutowired().SingleInstance();

            // Auto Activate Services
            builder.RegisterType<LiveLogService>().PropertiesAutowired().SingleInstance().AutoActivate();

            // TopLevel
            builder.RegisterType<TopLevelProxy>().SingleInstance();

            // Notification
            builder.Register<WindowNotificationManager>(ctx => ctx.Resolve<MainWindow>().Notifier).SingleInstance();
        }

        public void RegisterWindowsAndViewModels()
        {
            builder.RegisterType<MainWindowViewModel>().PropertiesAutowired().SingleInstance();
            builder.Register(static ctx => new MainWindow { DataContext = ctx.Resolve<MainWindowViewModel>() }).SingleInstance();

            builder.RegisterType<MainSplashWindowViewModel>().PropertiesAutowired().SingleInstance();
            builder.Register(static ctx => new MainSplashWindow { DataContext = ctx.Resolve<MainSplashWindowViewModel>() }).PropertiesAutowired().SingleInstance();

            builder.RegisterType<CrashWindowViewModel>().PropertiesAutowired().SingleInstance();
            builder.Register(static ctx => new CrashWindow { DataContext = ctx.Resolve<CrashWindowViewModel>() }).InstancePerDependency();
        }

        public void RegisterPerGameAppServices()
        {
            builder.RegisterType<SetupDialogService>().PropertiesAutowired().InstancePerLifetimeScope();
        }
    }
}