using Autofac;
using Serilog;

namespace MuseDashModTools.Core;

public static class ContainerBuilderExtension
{
    private static readonly string LogFileName = $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log";

    private　static void CreateLogger()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
#if DEBUG
            .WriteTo.Console()
#endif
            .WriteTo.File(new LogFileFormatter(),
                Path.Combine("Logs", LogFileName),
                rollingInterval: RollingInterval.Infinite)
            .CreateLogger();
    }
    public static void RegisterLogger(this ContainerBuilder builder)
    {
        CreateLogger();
        builder.RegisterInstance(Log.Logger).As<ILogger>().SingleInstance();
    }
    public static void RegisterCoreServices(this ContainerBuilder builder)
    {
        builder.RegisterType<JsonSerializationService>().As<IJsonSerializationService>().PropertiesAutowired().SingleInstance();
    }
}