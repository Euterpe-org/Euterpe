using Autofac;

namespace MuseDashModTools.Core.Extensions;

public static class CoreServiceExtensions
{
    public static void RegisterLogger(this ServiceCollection services, string logFileName)
    {
        services.AddLogging(x =>
        {
            x.ClearProviders();
#if DEBUG
            x.SetMinimumLevel(LogLevel.Trace);
            x.AddZLoggerConsole(options =>
            {
                options.ConfigureEnableAnsiEscapeCode = true;
                options.UseFormatter(() => new LogConsoleFormatter());
            });
#else
            x.SetMinimumLevel(LogLevel.Information);
#endif
            x.AddZLoggerFile((options, _) =>
            {
                options.FileShared = true;
                options.UseFormatter(() => new LogFileFormatter());
                return Path.Combine("Logs", logFileName);
            });
        });
    }

    public static void RegisterCoreServices(this ContainerBuilder builder)
    {
        builder.RegisterType<Setting>().SingleInstance();

        builder.RegisterType<FileSystemService>().As<IFileSystemService>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<JsonSerializationService>().As<IJsonSerializationService>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<LocalService>().As<ILocalService>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<MessageBoxService>().As<IMessageBoxService>().SingleInstance();
        builder.RegisterType<ModManageService>().As<IModManageService>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<SavingService>().As<ISavingService>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<UpdateService>().As<IUpdateService>().PropertiesAutowired().SingleInstance();

        // Platform Service
        if (OperatingSystem.IsWindows())
        {
            builder.RegisterType<WindowsService>().As<IPlatformService>()
                .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies)
                .SingleInstance();
        }
        else if (OperatingSystem.IsLinux())
        {
            builder.RegisterType<LinuxService>().As<IPlatformService>()
                .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies)
                .SingleInstance();
        }
        else if (OperatingSystem.IsMacOS())
        {
            builder.RegisterType<MacOsService>().As<IPlatformService>()
                .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies)
                .SingleInstance();
        }
    }
}