namespace MuseDashModTools.Extensions;

public static partial class ServiceExtensions
{
    public static void RegisterInternalServices(this ContainerBuilder builder)
    {
        // Self Services
        builder.RegisterType<NavigationService>().PropertiesAutowired().SingleInstance();

        builder.RegisterType<LocalizationService>().As<ILocalizationService>().PropertiesAutowired().SingleInstance();

        // TopLevel
        builder.Register(context => context.Resolve<MainWindow>().GetTopLevel()).As<TopLevel>().SingleInstance();
    }
}