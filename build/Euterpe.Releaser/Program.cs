using Euterpe.Releaser;

var app = ConsoleApp.Create()
    .ConfigureLogging(static logging => logging.AddZLoggerConsole())
    .ConfigureServices(static services =>
    {
        services.AddSingleton<ReleaseProcessRunner>();
        services.AddSingleton<RidReleaseStager>();
    });

app.Add<ReleaseCommands>();
await app.RunAsync(args);
