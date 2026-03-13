namespace Euterpe.Core.Extensions;

public static class CoreServiceExtensions
{
    extension(IServiceCollection services)
    {
        public void RegisterLogger(string logFileName)
        {
            services.AddSingleton<LiveLogProcessor>();
            services.AddLogging(x =>
            {
                x.ClearProviders();
#if DEBUG
                x.SetMinimumLevel(LogLevel.Debug);
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
                x.AddZLoggerLogProcessor((_, provider) => provider.GetRequiredService<LiveLogProcessor>());
            });
        }

        public void RegisterHttpClients()
        {
            services.AddHttpClient();
            services.AddHttpClient<TelemetryApiClient>(client =>
                    client.BaseAddress = new Uri($"{EuterpeApi.BaseUrl}{EuterpeApi.Telemetry.BasePath}"))
                .AddHttpMessageHandler<XRequestIdHandler>();
        }
    }

    extension(ContainerBuilder builder)
    {
        public void RegisterInstances()
        {
            builder.RegisterInstance(new DownloadService(
                    new DownloadConfiguration
                    {
                        ChunkCount = 8,
                        MaxTryAgainOnFailure = 4,
                        ParallelDownload = true,
                        BlockTimeout = 3000
                    }))
                .As<IDownloadService>();
        }

        public void RegisterCoreServices()
        {
            builder.RegisterType<Config>().SingleInstance();
            builder.RegisterType<WindowNotificationManager>().SingleInstance();

            builder.RegisterType<ArchiveService>().As<IArchiveService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<ChartManageService>().As<IChartManageService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<DownloadManager>().As<IDownloadManager>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<FileSystemService>().As<IFileSystemService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<FileSystemPickerService>().As<IFileSystemPickerService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<GameService>().As<IGameService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<LocalService>().As<ILocalService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<MessageBoxService>().As<IMessageBoxService>().SingleInstance();
            builder.RegisterType<ModManageService>().As<IModManageService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<NotificationService>().As<INotificationService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<ResourceService>().As<IResourceService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<SettingService>().As<ISettingService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<TelemetryService>().As<ITelemetryService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<GamePathService>().As<IGamePathService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<UpdateService>().As<IUpdateService>().PropertiesAutowired().SingleInstance();

            // Serialization Services
            builder.RegisterType<JsonSerializationService>().As<IJsonSerializationService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<VdfSerializationService>().As<IVdfSerializationService>().PropertiesAutowired().SingleInstance();

#pragma warning disable CA1416
            // Platform Service
#if WINDOWS
            builder.RegisterType<WindowsService>().As<IPlatformService>().PropertiesAutowired().SingleInstance();
#elif LINUX
            builder.RegisterType<LinuxService>().As<IPlatformService>().PropertiesAutowired().SingleInstance();
#elif MACOS
            builder.RegisterType<MacOsService>().As<IPlatformService>().PropertiesAutowired().SingleInstance();
#endif
#pragma warning restore CA1416
        }
    }
}