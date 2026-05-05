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
        public void RegisterAppCoreServices()
        {
            builder.RegisterType<AuthState>().SingleInstance();
            builder.RegisterType<Config>().PropertiesAutowired().SingleInstance();

            builder.RegisterType<AppDownloadManager>().As<IAppDownloadManager>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<AppLocalService>().As<IAppLocalService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<AppSettingService>().As<IAppSettingService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<ArchiveService>().As<IArchiveService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<AuthService>().As<IAuthService>().PropertiesAutowired().SingleInstance();
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

        public void RegisterPerGameCoreServices()
        {
            builder.RegisterType<MuseDashConfig>().AsSelf().As<GameConfig>().SingleInstance();
            builder.RegisterType<MuseDash2Config>().AsSelf().SingleInstance();

            builder.RegisterType<ChartManageService>().As<IChartManageService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<DependencyAcquireService>().As<IDependencyAcquireService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<GameDownloadManager>().As<IGameDownloadManager>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<GameLaunchService>().As<IGameLaunchService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<GameLocalService>().As<IGameLocalService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<GamePathService>().As<IGamePathService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<GameSettingService>().As<IGameSettingService>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<ModManageService>().As<IModManageService>().PropertiesAutowired().SingleInstance();

            // Wizard Steps
            builder.RegisterType<ChartingToolStep>().As<IWizardStep>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<DotNetRuntimeStep>().As<IWizardStep>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<DotNetSdkStep>().As<IWizardStep>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<EnvVariableStep>().As<IWizardStep>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<EssentialModsStep>().As<IWizardStep>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<MelonLoaderStep>().As<IWizardStep>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<ModTemplateStep>().As<IWizardStep>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<UninstallConflictsStep>().As<IWizardStep>().PropertiesAutowired().SingleInstance();

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
        }

        private void RegisterPerPlatformGameServices()
        {
#if WINDOWS
            builder.RegisterType<WindowsGameModTemplateInstaller>().As<IGameModTemplateInstaller>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<WindowsGamePathDiscovery>().As<IGamePathDiscovery>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<WindowsGamePathEnvironment>().As<IGamePathEnvironment>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<WindowsGameRuntimeInstaller>().As<IGameRuntimeInstaller>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<WindowsGameUidProvider>().As<IGameUidProvider>().PropertiesAutowired().SingleInstance();
#elif LINUX
            builder.RegisterType<LinuxGameModTemplateInstaller>().As<IGameModTemplateInstaller>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<LinuxGamePathDiscovery>().As<IGamePathDiscovery>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<LinuxGamePathEnvironment>().As<IGamePathEnvironment>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<LinuxGameRuntimeInstaller>().As<IGameRuntimeInstaller>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<LinuxGameUidProvider>().As<IGameUidProvider>().PropertiesAutowired().SingleInstance();
#elif MACOS
            builder.RegisterType<MacOsGameModTemplateInstaller>().As<IGameModTemplateInstaller>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<MacOsGamePathDiscovery>().As<IGamePathDiscovery>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<MacOsGamePathEnvironment>().As<IGamePathEnvironment>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<MacOsGameRuntimeInstaller>().As<IGameRuntimeInstaller>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<MacOsGameUidProvider>().As<IGameUidProvider>().PropertiesAutowired().SingleInstance();
#endif
#pragma warning restore CA1416
        }
    }
}