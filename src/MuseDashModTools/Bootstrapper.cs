using MuseDashModTools.Core;
using MuseDashModTools.Views.Dialogs;

namespace MuseDashModTools;

public static class Bootstrapper
{
    private static readonly ContainerBuilder _builder = new();

    /// <summary>
    ///     Register all instances, services and view models
    ///     Configure static resolvers
    /// </summary>
    public static IContainer Register()
    {
        RegisterInstances();
        RegisterComponents();
        RegisterServices();
        RegisterViews();
        RegisterViewModels();

        return _builder.Build();
    }

    /// <summary>
    ///     Register instances
    /// </summary>
    private static void RegisterInstances()
    {
        _builder.RegisterInstance(new HttpClient());
        _builder.RegisterInstance(new MultiThreadDownloader(
            new DownloadConfiguration
            {
                ChunkCount = 8,
                MaxTryAgainOnFailover = 4,
                ParallelCount = 4,
                ParallelDownload = true,
                Timeout = 3000
            }));
    }

    /// <summary>
    ///     Register Frequently-Used Components
    /// </summary>
    private static void RegisterComponents()
    {
        _builder.RegisterType<Setting>().SingleInstance();
    }

    /// <summary>
    ///     Register all services
    /// </summary>
    private static void RegisterServices()
    {
        // Self Services
        _builder.RegisterType<NavigationService>().PropertiesAutowired().SingleInstance();

        // Interface Services
        _builder.RegisterType<FileSystemPickerService>().As<IFileSystemPickerService>().PropertiesAutowired().SingleInstance();
        _builder.RegisterType<LocalService>().As<ILocalService>().PropertiesAutowired().SingleInstance();
        _builder.RegisterType<ModManageService>().As<IModManageService>().PropertiesAutowired().SingleInstance();
        _builder.RegisterType<SavingService>().As<ISavingService>().PropertiesAutowired().SingleInstance();
        _builder.RegisterCoreServices();

        // Download Services
        _builder.RegisterType<CustomDownloadService>().As<ICustomDownloadService>().PropertiesAutowired().SingleInstance();
        _builder.RegisterType<GitHubDownloadService>().As<IGitHubDownloadService>().PropertiesAutowired().SingleInstance();
        _builder.RegisterType<GitHubMirrorDownloadService>().As<IGitHubMirrorDownloadService>().PropertiesAutowired().SingleInstance();
        _builder.RegisterType<DownloadManager>().As<IDownloadManager>().PropertiesAutowired().SingleInstance();

        // Platform Service
        if (OperatingSystem.IsWindows())
        {
            _builder.RegisterType<WindowsService>().As<IPlatformService>().PropertiesAutowired().SingleInstance();
        }
        else if (OperatingSystem.IsLinux())
        {
            _builder.RegisterType<LinuxService>().As<IPlatformService>().PropertiesAutowired().SingleInstance();
        }
        // for macos development
        else if (OperatingSystem.IsMacOS())
        {
            _builder.RegisterType<MacOsService>().As<IPlatformService>().PropertiesAutowired().SingleInstance();
        }
    }

    private static void RegisterViews()
    {
        // Window
        _builder.Register<MainWindow>(ctx => new MainWindow { DataContext = ctx.Resolve<MainWindowViewModel>() }).SingleInstance();

        // Pages
        _builder.Register<HomePage>(ctx => new HomePage { DataContext = ctx.Resolve<HomePageViewModel>() }).SingleInstance();
        _builder.Register<ModManagePage>(ctx => new ModManagePage { DataContext = ctx.Resolve<ModManagePageViewModel>() }).SingleInstance();
        _builder.Register<ModDevelopPage>(ctx => new ModDevelopPage { DataContext = ctx.Resolve<ModDevelopPageViewModel>() }).SingleInstance();
        _builder.Register<ChartManagePage>(ctx => new ChartManagePage { DataContext = ctx.Resolve<ChartManagePageViewModel>() }).SingleInstance();
        _builder.Register<ChartToolkitPage>(ctx => new ChartToolkitPage { DataContext = ctx.Resolve<ChartToolkitPageViewModel>() }).SingleInstance();
        _builder.Register<LogAnalysisPage>(ctx => new LogAnalysisPage { DataContext = ctx.Resolve<LogAnalysisPageViewModel>() }).SingleInstance();
        _builder.Register<AboutPage>(ctx => new AboutPage { DataContext = ctx.Resolve<AboutPageViewModel>() }).SingleInstance();
        _builder.Register<SettingPage>(ctx => new SettingPage { DataContext = ctx.Resolve<SettingPageViewModel>() }).SingleInstance();

        // Dialog
        _builder.Register<DownloadDialog>(ctx => new DownloadDialog { DataContext = ctx.Resolve<DownloadDialogViewModel>() }).SingleInstance();
    }

    /// <summary>
    ///     Register all view models
    /// </summary>
    private static void RegisterViewModels()
    {
        // Window
        _builder.RegisterType<MainWindowViewModel>().PropertiesAutowired().SingleInstance();

        // Pages
        _builder.RegisterType<HomePageViewModel>().PropertiesAutowired().SingleInstance();
        _builder.RegisterType<ModManagePageViewModel>().PropertiesAutowired().SingleInstance();
        _builder.RegisterType<ModDevelopPageViewModel>().PropertiesAutowired().SingleInstance();
        _builder.RegisterType<ChartManagePageViewModel>().PropertiesAutowired().SingleInstance();
        _builder.RegisterType<ChartToolkitPageViewModel>().PropertiesAutowired().SingleInstance();
        _builder.RegisterType<LogAnalysisPageViewModel>().PropertiesAutowired().SingleInstance();
        _builder.RegisterType<AboutPageViewModel>().PropertiesAutowired().SingleInstance();
        _builder.RegisterType<SettingPageViewModel>().PropertiesAutowired().SingleInstance();

        // Dialog
        _builder.RegisterType<DownloadDialogViewModel>().PropertiesAutowired().SingleInstance();
    }
}