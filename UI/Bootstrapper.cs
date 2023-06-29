using System.Net.Http;
using Autofac;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Contracts.ViewModels;
using MuseDashModToolsUI.Extensions;
using MuseDashModToolsUI.Services;
using MuseDashModToolsUI.ViewModels;
using MuseDashModToolsUI.ViewModels.Dialogs;
using MuseDashModToolsUI.ViewModels.Tabs;
using Serilog;

namespace MuseDashModToolsUI;

public static class Bootstrapper
{
    public static void Register()
    {
        var builder = new ContainerBuilder();
        builder.RegisterInstance(Log.Logger).As<ILogger>().SingleInstance();
        builder.RegisterInstance(new HttpClient());

        // Services
        builder.RegisterType<MessageBoxService>().PropertiesAutowired().As<IMessageBoxService>();
        builder.RegisterType<GitHubService>().As<IGitHubService>().PropertiesAutowired();
        builder.RegisterType<SettingService>().As<ISettingService>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<LocalizationService>().As<ILocalizationService>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<LocalService>().As<ILocalService>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<ModService>().As<IModService>().PropertiesAutowired().SingleInstance();

        // View Models
        builder.RegisterType<ProjectWindowViewModel>().As<IProjectWindowViewModel>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<DownloadWindowViewModel>().As<IDownloadWindowViewModel>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<ModManageViewModel>().As<IModManageViewModel>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<SettingsViewModel>().As<ISettingsViewModel>().SingleInstance();
        builder.RegisterType<UpdateTextService>().As<IUpdateTextService>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<MainWindowViewModel>().As<IMainWindowViewModel>().SingleInstance();

        var container = builder.Build();
        DependencyInjectionExtension.Resolver = type => container.Resolve(type!);
        LocalizeExtensions.LocalizationService = container.Resolve<ILocalizationService>();
    }
}