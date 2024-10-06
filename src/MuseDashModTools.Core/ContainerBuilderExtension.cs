using Autofac;

namespace MuseDashModTools.Core;

public static class ContainerBuilderExtension
{
    public static void RegisterCoreServices(this ContainerBuilder builder)
    {
        builder.RegisterType<JsonSerializationService>().As<IJsonSerializationService>().PropertiesAutowired().SingleInstance();
    }
}