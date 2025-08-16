using Microsoft.Extensions.DependencyInjection;

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

var serviceProvider = services.BuildServiceProvider();
await using (serviceProvider.ConfigureAwait(false))
{
    ConsoleApp.ServiceProvider = serviceProvider;

    var app = ConsoleApp.Create();
    app.Add<Commands>();
    await app.RunAsync(args).ConfigureAwait(false);
}