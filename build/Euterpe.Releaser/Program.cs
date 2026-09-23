using Euterpe.Releaser;

return new CakeHost()
    .UseContext<ReleaseContext>()
    .ConfigureServices(static services => services.RegisterReleaserServices())
    .Run(args);
