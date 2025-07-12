using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace MuseDashModTools;

public static class IocContainer
{
    private static IContainer Container { get; set; } = null!;

    public static T Resolve<T>() where T : notnull => Container.Resolve<T>();

    public static void ConfigureContainer(string logFileName)
    {
        var services = new ServiceCollection();
        services.RegisterLogger(logFileName);

        var builder = new ContainerBuilder();
        builder.RegisterInstances();
        builder.RegisterCoreServices();
        builder.RegisterInternalServices();
        builder.RegisterLazyProxies();
        builder.RegisterViewAndViewModels();

        builder.Populate(services);
        Container = builder.Build();
    }
}