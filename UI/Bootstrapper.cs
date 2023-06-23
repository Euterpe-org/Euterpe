using System.Net.Http;
using Autofac;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Contracts.ViewModels;
using MuseDashModToolsUI.Extensions;
using MuseDashModToolsUI.Services;
using MuseDashModToolsUI.ViewModels;
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
        builder.RegisterType<DialogueService>().As<IDialogueService>();
        builder.RegisterType<GitHubService>().As<IGitHubService>();
        builder.RegisterType<SettingService>().As<ISettingService>().SingleInstance();
        builder.RegisterType<LocalizationService>().As<ILocalizationService>().SingleInstance();
        builder.RegisterType<DownloadWindowViewModel>().As<IDownloadWindowViewModel>().SingleInstance();
        builder.RegisterType<LocalService>().As<ILocalService>().SingleInstance();
        builder.RegisterType<ModService>().As<IModService>().SingleInstance();
        builder.RegisterType<ModManageViewModel>().As<IModManageViewModel>().SingleInstance();
        builder.RegisterType<SettingsViewModel>().As<ISettingsViewModel>().SingleInstance();
        builder.RegisterType<UpdateTextService>().As<IUpdateTextService>().SingleInstance();
        builder.RegisterType<MainWindowViewModel>().As<IMainWindowViewModel>().SingleInstance();

        var container = builder.Build();
        DependencyInjectionExtension.Resolver = type => container.Resolve(type!);
        LocalizeExtensions.LocalizationService = container.Resolve<ILocalizationService>();
    }
}