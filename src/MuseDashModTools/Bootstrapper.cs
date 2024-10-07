using MuseDashModTools.Views.Dialogs;

namespace MuseDashModTools;

public static class Bootstrapper
{
    private static void RegisterViews(this ContainerBuilder builder)
    {
        // Window
        builder.Register<MainWindow>(ctx => new MainWindow { DataContext = ctx.Resolve<MainWindowViewModel>() }).SingleInstance();

        // Pages
        builder.Register<HomePage>(ctx => new HomePage { DataContext = ctx.Resolve<HomePageViewModel>() }).SingleInstance();
        builder.Register<ModManagePage>(ctx => new ModManagePage { DataContext = ctx.Resolve<ModManagePageViewModel>() }).SingleInstance();
        builder.Register<ModDevelopPage>(ctx => new ModDevelopPage { DataContext = ctx.Resolve<ModDevelopPageViewModel>() }).SingleInstance();
        builder.Register<ChartManagePage>(ctx => new ChartManagePage { DataContext = ctx.Resolve<ChartManagePageViewModel>() }).SingleInstance();
        builder.Register<ChartToolkitPage>(ctx => new ChartToolkitPage { DataContext = ctx.Resolve<ChartToolkitPageViewModel>() }).SingleInstance();
        builder.Register<LogAnalysisPage>(ctx => new LogAnalysisPage { DataContext = ctx.Resolve<LogAnalysisPageViewModel>() }).SingleInstance();
        builder.Register<AboutPage>(ctx => new AboutPage { DataContext = ctx.Resolve<AboutPageViewModel>() }).SingleInstance();
        builder.Register<SettingPage>(ctx => new SettingPage { DataContext = ctx.Resolve<SettingPageViewModel>() }).SingleInstance();

        // Dialog
        builder.Register<DownloadDialog>(ctx => new DownloadDialog { DataContext = ctx.Resolve<DownloadDialogViewModel>() }).SingleInstance();
    }

    /// <summary>
    ///     Register all view models
    /// </summary>
    private static void RegisterViewModels(this ContainerBuilder builder)
    {
        // Window
        builder.RegisterType<MainWindowViewModel>().PropertiesAutowired().SingleInstance();

        // Pages
        builder.RegisterType<HomePageViewModel>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<ModManagePageViewModel>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<ModDevelopPageViewModel>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<ChartManagePageViewModel>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<ChartToolkitPageViewModel>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<LogAnalysisPageViewModel>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<AboutPageViewModel>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<SettingPageViewModel>().PropertiesAutowired().SingleInstance();

        // Dialog
        builder.RegisterType<DownloadDialogViewModel>().PropertiesAutowired().SingleInstance();
    }
}