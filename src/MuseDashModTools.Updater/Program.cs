using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ZLogger;

var services = new ServiceCollection();
services.AddSingleton<ILocalService, LocalService>();
services.AddLogging(x =>
{
    x.ClearProviders();
#if DEBUG
    x.SetMinimumLevel(LogLevel.Trace);
#else
    x.SetMinimumLevel(LogLevel.Information);
#endif
    x.AddZLoggerConsole();
});

await using var serviceProvider = services.BuildServiceProvider();
ConsoleApp.ServiceProvider = serviceProvider;

var app = ConsoleApp.Create();

await app.RunAsync(args).ConfigureAwait(false);