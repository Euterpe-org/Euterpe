using System;
using System.Net.Http;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Contracts.ViewModels;
using MuseDashModToolsUI.Services;
using MuseDashModToolsUI.ViewModels;
using MuseDashModToolsUI.ViewModels.Tabs;
using Serilog;
using Splat;
using ILogger = Serilog.ILogger;

namespace MuseDashModToolsUI;

public static class Bootstrapper
{
    public static void Register(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
    {
        services.RegisterConstant(Log.Logger);

        // Dialog Service
        services.Register<IDialogueService>(() => new DialogueService());

        // Github Service
        services.Register<IGitHubService>(() => new GitHubService(
            new HttpClient(),
            resolver.GetRequiredService<IDialogueService>(),
            resolver.GetRequiredService<ILogger>()));

        // Setting Service
        services.RegisterConstant<ISettingService>(new SettingService(
            resolver.GetRequiredService<IDialogueService>(),
            resolver.GetRequiredService<ILogger>()));

        // Localization Service
        services.RegisterConstant<ILocalizationService>(new LocalizationService(
            resolver.GetRequiredService<ILogger>(),
            resolver.GetRequiredService<ISettingService>()));

        // Download Window View Model
        services.RegisterLazySingleton<IDownloadWindowViewModel>(() => new DownloadWindowViewModel(
            resolver.GetRequiredService<IDialogueService>(),
            resolver.GetRequiredService<IGitHubService>(),
            resolver.GetRequiredService<ILogger>(),
            resolver.GetRequiredService<ISettingService>()
        ));

        // Local Service
        services.RegisterConstant<ILocalService>(new LocalService(
            resolver.GetRequiredService<IDialogueService>(),
            resolver.GetRequiredService<IDownloadWindowViewModel>(),
            resolver.GetRequiredService<ILogger>(),
            resolver.GetRequiredService<ISettingService>()));

        // Mod Service
        services.RegisterConstant<IModService>(new ModService(
            resolver.GetRequiredService<IDialogueService>(),
            resolver.GetRequiredService<IGitHubService>(),
            resolver.GetRequiredService<ILocalService>(),
            resolver.GetRequiredService<ILogger>(),
            resolver.GetRequiredService<ISettingService>()
        ));

        // Mod Manage UserControl View Model
        services.RegisterLazySingleton<IModManageViewModel>(() => new ModManageViewModel(
            resolver.GetRequiredService<IGitHubService>(),
            resolver.GetRequiredService<ILocalService>(),
            resolver.GetRequiredService<ILogger>(),
            resolver.GetRequiredService<IModService>(),
            resolver.GetRequiredService<ISettingService>()));

        // Settings UserControl View Model
        services.RegisterLazySingleton<ISettingsViewModel>(() => new SettingsViewModel(
            resolver.GetRequiredService<ILocalizationService>(),
            resolver.GetRequiredService<IModManageViewModel>(),
            resolver.GetRequiredService<ILogger>(),
            resolver.GetRequiredService<ISettingService>()));

        // Main Window View Model
        services.RegisterLazySingleton<IMainWindowViewModel>(() => new MainWindowViewModel(
            resolver.GetRequiredService<ILogger>(),
            resolver.GetRequiredService<ISettingService>()));
    }

    public static TService GetRequiredService<TService>(this IReadonlyDependencyResolver resolver)
    {
        var service = resolver.GetService<TService>();
        if (service is null) // Splat is not able to resolve type for us
            throw new InvalidOperationException(
                $"Failed to resolve object of type {typeof(TService)}"); // throw error with detailed description

        return service; // return instance if not null
    }
}