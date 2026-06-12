using Microsoft.Extensions.DependencyInjection;

var app = ConsoleApp.Create()
    .ConfigureServices(services => services.AddSingleton<ILocalService, LocalService>())
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
