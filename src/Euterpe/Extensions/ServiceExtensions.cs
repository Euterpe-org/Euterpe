using Euterpe.Core.Proxies;

namespace Euterpe.Extensions;

public static partial class ServiceExtensions
{
    extension(ContainerBuilder builder)
    {
        public void RegisterInternalServices()
        {
            // Self Services
            builder.RegisterType<AppInitializer>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<DeepLinkService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<GameSwitcher>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<LocalizationService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<NavigationService>().PropertiesAutowired().SingleInstance();

            // Auto Activate Services
            builder.RegisterType<LiveLogService>().PropertiesAutowired().SingleInstance().AutoActivate();

            // TopLevel
            builder.RegisterType<TopLevelProxy>().SingleInstance();
        }

        public void RegisterPerGameAppServices()
        {
            builder.RegisterType<SetupDialogService>().PropertiesAutowired().InstancePerLifetimeScope();
        }
    }
}