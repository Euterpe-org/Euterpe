using Euterpe.Core.Http.Handlers;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace Euterpe.Core.Extensions;

public static class CoreServiceExtensions
{
    private static void ConfigureResilience(HttpStandardResilienceOptions options)
    {
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
        options.Retry.MaxRetryAttempts = 3;
        options.Retry.Delay = TimeSpan.FromMilliseconds(500);
        options.Retry.BackoffType = DelayBackoffType.Exponential;
        options.Retry.UseJitter = true;
    }

    extension(IServiceCollection services)
    {
        public void RegisterLogger(string logFilePath)
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
                    return logFilePath;
                });
                x.AddZLoggerLogProcessor((_, provider) => provider.GetRequiredService<LiveLogProcessor>());
            });
        }

        public void RegisterHttpClients()
        {
            services.AddTransient<XRequestIdHandler>();
            services.AddTransient<AuthHeaderHandler>();
            services.AddTransient<LoggingHandler>();
            services.AddTransient<ServerErrorHandler>();
            services.AddTransient<TokenQueryHandler>();

            services.AddSingleton<IDownloadService>(sp =>
            {
                var handler = sp.GetRequiredService<TokenQueryHandler>();
                handler.InnerHandler = new SocketsHttpHandler();

                return new DownloadService(new DownloadConfiguration
                {
                    ChunkCount = 8,
                    MaxTryAgainOnFailure = 4,
                    ParallelDownload = true,
                    BlockTimeout = 3000,
                    CustomHttpMessageHandlerFactory = () => handler
                });
            });

            services.ConfigureHttpClientDefaults(builder =>
            {
                builder.AddHttpMessageHandler<LoggingHandler>();
                builder.AddHttpMessageHandler<ServerErrorHandler>();
            });
            services.AddHttpClient();
            services.AddHttpClient<EuterpeDownloadClient>().AddHttpMessageHandler<TokenQueryHandler>();

            services.AddEuterpeRefitClient<IEuterpeAuthClient>(nameof(EuterpeApi.Auth), EuterpeApi.Auth.BasePath);
            services.AddEuterpeRefitClient<IEuterpeAccountClient>(nameof(EuterpeApi.Account), EuterpeApi.Account.BasePath, true);
            services.AddEuterpeRefitClient<IEuterpeDistributionClient>(nameof(EuterpeApi.Distribution), EuterpeApi.Distribution.BasePath, true);
            services.AddEuterpeRefitClient<IEuterpeModClient>(nameof(EuterpeApi.Mods), EuterpeApi.Mods.BasePath, true)
                .AddStandardResilienceHandler(ConfigureResilience);
            services.AddEuterpeRefitClient<IEuterpeChartClient>(nameof(EuterpeApi.Charts), EuterpeApi.Charts.BasePath, true)
                .AddStandardResilienceHandler(ConfigureResilience);
            services.AddEuterpeRefitClient<ITelemetryApiClient>(nameof(EuterpeApi.Telemetry), EuterpeApi.Telemetry.BasePath);
        }
    }

    extension(ContainerBuilder builder)
    {
        public void RegisterCoreServices()
        {
            builder.RegisterType<AuthState>().SingleInstance();
            builder.RegisterType<Config>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<MuseDashConfig>().AsSelf().As<GameConfig>().Keyed<GameConfig>(GameId.MuseDash).SingleInstance();

            builder.RegisterType<AuthService>().As<IAuthService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<ArchiveService>().As<IArchiveService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<ChartManageService>().As<IChartManageService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<DependencyAcquireService>().As<IDependencyAcquireService>().PropertiesAutowired().SingleInstance();
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

            // Wizard Steps
            builder.RegisterType<MelonLoaderStep>().As<IWizardStep>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<EssentialModsStep>().As<IWizardStep>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<UninstallConflictsStep>().As<IWizardStep>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<ChartingToolStep>().As<IWizardStep>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<DotNetSdkStep>().As<IWizardStep>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<ModTemplateStep>().As<IWizardStep>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<EnvVariableStep>().As<IWizardStep>().PropertiesAutowired().SingleInstance();

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