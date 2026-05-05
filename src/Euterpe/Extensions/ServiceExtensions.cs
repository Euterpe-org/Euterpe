using Euterpe.Core.Proxies;
using WindowNotificationManager = Ursa.Controls.WindowNotificationManager;

namespace Euterpe.Extensions;

public static partial class ServiceExtensions
{
    public static void RegisterInternalServices(this ContainerBuilder builder)
    {
        // Self Services
        builder.RegisterType<DeepLinkService>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<GameSwitcher>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<NavigationService>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<LocalizationService>().PropertiesAutowired().SingleInstance();

        // Auto Activate Services
        builder.RegisterType<LiveLogService>().PropertiesAutowired().SingleInstance().AutoActivate();

        // TopLevel
        builder.Register<TopLevel>(ctx => ctx.Resolve<MainWindow>()).SingleInstance();
        builder.RegisterType<TopLevelProxy>().SingleInstance();

        // Notification
        builder.Register<WindowNotificationManager>(ctx => ctx.Resolve<MainWindow>().Notifier).SingleInstance();
    }
}