using Autofac;

namespace MuseDashModTools.Core.Extensions;

public static class ContainerBuilderExtensions
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
        builder.RegisterType<Setting>().SingleInstance();

        builder.RegisterType<FileSystemService>().As<IFileSystemService>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<JsonSerializationService>().As<IJsonSerializationService>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<LocalService>().As<ILocalService>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<SavingService>().As<ISavingService>().PropertiesAutowired().SingleInstance();

        // Platform Service
        if (OperatingSystem.IsWindows())
        {
            builder.RegisterType<WindowsService>().As<IPlatformService>().PropertiesAutowired().SingleInstance();
        }
        else if (OperatingSystem.IsLinux())
        {
            builder.RegisterType<LinuxService>().As<IPlatformService>().PropertiesAutowired().SingleInstance();
        }
        else if (OperatingSystem.IsMacOS())
        {
            builder.RegisterType<MacOsService>().As<IPlatformService>().PropertiesAutowired().SingleInstance();
        }
    }
}