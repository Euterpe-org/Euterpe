using Microsoft.Extensions.DependencyInjection;

var app = ConsoleApp.Create()
    .ConfigureServices(services =>
    {
        services.AddSingleton<ILocalService, LocalService>();
#if WINDOWS
        services.AddSingleton<IPlatformInfo, WindowsPlatformInfo>();
#elif LINUX
        services.AddSingleton<IPlatformInfo, LinuxPlatformInfo>();
#elif MACOS
        services.AddSingleton<IPlatformInfo, MacOsPlatformInfo>();
#endif
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
#if DEBUG
        logging.SetMinimumLevel(LogLevel.Trace);
#else
        logging.SetMinimumLevel(LogLevel.Information);
#endif
        logging.AddZLoggerConsole();
    });

app.Add<Commands>();
await app.RunAsync(args).ConfigureAwait(false);
