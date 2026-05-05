using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Euterpe;

public static class IocContainer
{
    private static IContainer Container { get; set; } = null!;

    public static T Resolve<T>() where T : notnull => Container.Resolve<T>();

    public static void ConfigureContainer(string logFileName)
    {
        var services = new ServiceCollection();
        services.RegisterLogger(logFileName);
        services.RegisterHttpClients();

        var builder = new ContainerBuilder();
        builder.RegisterAppCoreServices();
        builder.RegisterPerGameCoreServices();
        builder.RegisterInternalServices();
        builder.RegisterAppViewsAndViewModels();
        builder.RegisterPerGameViewsAndViewModels();

        builder.Populate(services);
        Container = builder.Build();
    }
}