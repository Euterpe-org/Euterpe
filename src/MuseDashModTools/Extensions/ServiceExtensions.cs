namespace MuseDashModTools.Extensions;

public static partial class ServiceExtensions
{
    public static void RegisterInstances(this ContainerBuilder builder)
    {
        builder.RegisterInstance(new HttpClient());
        builder.RegisterInstance(new MultiThreadDownloader(
            new DownloadConfiguration
            {
                ChunkCount = 8,
                MaxTryAgainOnFailover = 4,
                ParallelCount = 4,
                ParallelDownload = true,
                Timeout = 3000
            }));
    }

    public static void RegisterInternalServices(this ContainerBuilder builder)
    {
        // Self Services
        builder.RegisterType<NavigationService>().PropertiesAutowired().SingleInstance();

        // Interface Services
        builder.RegisterType<FileSystemPickerService>().As<IFileSystemPickerService>()
            .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies)
            .SingleInstance();
        builder.RegisterType<ResourceService>().As<IResourceService>().PropertiesAutowired().SingleInstance();

        // Download Services
        builder.RegisterType<CustomDownloadService>().As<ICustomDownloadService>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<GitHubDownloadService>().As<IGitHubDownloadService>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<GitHubMirrorDownloadService>().As<IGitHubMirrorDownloadService>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<DownloadManager>().As<IDownloadManager>().PropertiesAutowired().SingleInstance();

        builder.Register(context => context.Resolve<MainWindow>().GetTopLevel()).As<TopLevel>().SingleInstance();
    }
}