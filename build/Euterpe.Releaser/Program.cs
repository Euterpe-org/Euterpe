using Euterpe.Releaser;

var logConfig = new LogManagerConfig
{
    RootLogger =
    {
        MinimumLevel = LogLevel.Info,
        Writers =
        {
            new StreamLogWriter(Console.OpenStandardOutput())
            {
                AutoFlush = true
            }
        }
    }
};
LogManager.Initialize(logConfig);

try
{
    var app = ConsoleApp.Create()
        .ConfigureServices(static services =>
        {
            services.AddSingleton<ReleaseProcessRunner>();
            services.AddSingleton<RidReleaseStager>();
        });

    app.Add<ReleaseCommands>();
    await app.RunAsync(args);
}
finally
{
    LogManager.Shutdown();
}
