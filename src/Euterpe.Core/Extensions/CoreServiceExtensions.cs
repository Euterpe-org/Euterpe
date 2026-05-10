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
        public void RegisterLogger()
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
                    return LogFilePath;
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
        public void RegisterAppCoreServices()
        {
            builder.RegisterType<AuthState>().SingleInstance();
            builder.RegisterType<Config>().PropertiesAutowired().SingleInstance();

            builder.RegisterType<MuseDashConfig>().AsSelf().SingleInstance();
            builder.RegisterType<MuseDash2Config>().AsSelf().SingleInstance();

            builder.RegisterType<AppDownloadManager>().As<IAppDownloadManager>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<AppLocalService>().As<IAppLocalService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<AppSettingService>().As<IAppSettingService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<ArchiveService>().As<IArchiveService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<AuthService>().As<IAuthService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<DialogService>().As<IDialogService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<FileSystemService>().As<IFileSystemService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<FileSystemPickerService>().As<IFileSystemPickerService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<JsonSerializationService>().As<IJsonSerializationService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<MessageBoxService>().As<IMessageBoxService>().SingleInstance();
            builder.RegisterType<NotificationService>().As<INotificationService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<ResourceService>().As<IResourceService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<TelemetryService>().As<ITelemetryService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<UpdateService>().As<IUpdateService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<VdfSerializationService>().As<IVdfSerializationService>().PropertiesAutowired().SingleInstance();

            builder.RegisterPerPlatformAppServices();
        }

        public void RegisterPerGameCoreServices(GameId activeGame)
        {
            switch (activeGame)
            {
                case GameId.MuseDash2:
                    builder.Register<GameConfig>(static ctx => ctx.Resolve<MuseDash2Config>()).InstancePerLifetimeScope();
                    break;
                default:
                    builder.Register<GameConfig>(static ctx => ctx.Resolve<MuseDashConfig>()).InstancePerLifetimeScope();
                    break;
            }

            builder.RegisterType<SetupState>().AsSelf().InstancePerLifetimeScope();

            builder.RegisterType<ChartManageService>().As<IChartManageService>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<DependencyAcquireService>().As<IDependencyAcquireService>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<GameDownloadManager>().As<IGameDownloadManager>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<GameLaunchService>().As<IGameLaunchService>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<GameLocalService>().As<IGameLocalService>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<GamePathService>().As<IGamePathService>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<GameSettingService>().As<IGameSettingService>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<ModManageService>().As<IModManageService>().PropertiesAutowired().InstancePerLifetimeScope();

            // Setup Steps
            builder.RegisterType<ChartingToolStep>().As<ISetupStep>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<DotNetRuntimeStep>().As<ISetupStep>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<DotNetSdkStep>().As<ISetupStep>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<EnvVariableStep>().As<ISetupStep>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<EssentialModsStep>().As<ISetupStep>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<MelonLoaderStep>().As<ISetupStep>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<ModTemplateStep>().As<ISetupStep>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<UninstallConflictsStep>().As<ISetupStep>().PropertiesAutowired().InstancePerLifetimeScope();

            builder.RegisterPerPlatformGameServices();
        }

        private void RegisterPerPlatformAppServices()
        {
#pragma warning disable CA1416
#if WINDOWS
            builder.RegisterType<WindowsDeepLinkSetup>().As<IDeepLinkSetup>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<WindowsDotNetSdkInstaller>().As<IDotNetSdkInstaller>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<WindowsLauncher>().As<IPlatformLauncher>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<WindowsPlatformInfo>().As<IPlatformInfo>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<WindowsSecureStorage>().As<IPlatformSecureStorage>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<WindowsSteamPathDiscovery>().As<ISteamPathDiscovery>().PropertiesAutowired().SingleInstance();
#elif LINUX
            builder.RegisterType<LinuxDeepLinkSetup>().As<IDeepLinkSetup>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<LinuxDotNetSdkInstaller>().As<IDotNetSdkInstaller>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<LinuxLauncher>().As<IPlatformLauncher>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<LinuxPlatformInfo>().As<IPlatformInfo>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<LinuxSecureStorage>().As<IPlatformSecureStorage>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<LinuxSteamPathDiscovery>().As<ISteamPathDiscovery>().PropertiesAutowired().SingleInstance();
#elif MACOS
            builder.RegisterType<MacOsDeepLinkSetup>().As<IDeepLinkSetup>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<MacOsDotNetSdkInstaller>().As<IDotNetSdkInstaller>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<MacOsLauncher>().As<IPlatformLauncher>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<MacOsPlatformInfo>().As<IPlatformInfo>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<MacOsSecureStorage>().As<IPlatformSecureStorage>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<MacOsSteamPathDiscovery>().As<ISteamPathDiscovery>().PropertiesAutowired().SingleInstance();
#endif
#pragma warning restore CA1416
        }

        private void RegisterPerPlatformGameServices()
        {
#pragma warning disable CA1416
#if WINDOWS
            builder.RegisterType<WindowsGameModTemplateInstaller>().As<IGameModTemplateInstaller>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<WindowsGamePathDiscovery>().As<IGamePathDiscovery>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<WindowsGamePathEnvironment>().As<IGamePathEnvironment>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<WindowsGameRuntimeInstaller>().As<IGameRuntimeInstaller>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<WindowsGameUidProvider>().As<IGameUidProvider>().PropertiesAutowired().InstancePerLifetimeScope();
#elif LINUX
            builder.RegisterType<LinuxGameModTemplateInstaller>().As<IGameModTemplateInstaller>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<LinuxGamePathDiscovery>().As<IGamePathDiscovery>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<LinuxGamePathEnvironment>().As<IGamePathEnvironment>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<LinuxGameRuntimeInstaller>().As<IGameRuntimeInstaller>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<LinuxGameUidProvider>().As<IGameUidProvider>().PropertiesAutowired().InstancePerLifetimeScope();
#elif MACOS
            builder.RegisterType<MacOsGameModTemplateInstaller>().As<IGameModTemplateInstaller>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<MacOsGamePathDiscovery>().As<IGamePathDiscovery>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<MacOsGamePathEnvironment>().As<IGamePathEnvironment>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<MacOsGameRuntimeInstaller>().As<IGameRuntimeInstaller>().PropertiesAutowired().InstancePerLifetimeScope();
            builder.RegisterType<MacOsGameUidProvider>().As<IGameUidProvider>().PropertiesAutowired().InstancePerLifetimeScope();
#endif
#pragma warning restore CA1416
        }
    }
}