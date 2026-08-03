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
            builder.RegisterType<SystemActivationService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<GameSwitcher>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<LocalizationService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<NavigationService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<UpdateDialogService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<UserGuideService>().PropertiesAutowired().SingleInstance();

            builder.RegisterType<UpdateDialogViewModel>().PropertiesAutowired().InstancePerDependency();

            // Auto Activate Services
            builder.RegisterType<LiveLogService>().PropertiesAutowired().SingleInstance().AutoActivate();

            // TopLevel
            builder.RegisterType<TopLevelProxy>().PropertiesAutowired().SingleInstance();
        }

        public void RegisterPerGameAppServices()
        {
            builder.RegisterType<ProgressDialogService>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<SetupDialogService>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<ShareImportDialogService>().PropertiesAutowired().InstancePerLifetimeScope();
        }
    }
}
