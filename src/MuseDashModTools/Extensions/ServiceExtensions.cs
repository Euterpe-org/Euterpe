namespace MuseDashModTools.Extensions;

public static partial class ServiceExtensions
{
    public static void RegisterInternalServices(this ContainerBuilder builder)
    {
        // Self Services
        builder.RegisterType<NavigationService>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<LocalizationService>().PropertiesAutowired().SingleInstance();

        // TopLevel
        builder.Register<TopLevel>(context => context.Resolve<MainWindow>().GetTopLevel()).SingleInstance();
    }
}