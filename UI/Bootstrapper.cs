using System;
using System.Net.Http;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Contracts.ViewModels;
using MuseDashModToolsUI.Services;
using MuseDashModToolsUI.ViewModels;
using Splat;

namespace MuseDashModToolsUI;

public static class Bootstrapper
{
    public static void Register(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
    {
        // Dialog Service
        services.Register<IDialogueService>(() => new DialogueService());

        // Github Service
        services.Register<IGitHubService>(() =>
            new GitHubService(new HttpClient(), resolver.GetRequiredService<IDialogueService>()));

        // Setting Service
        services.RegisterConstant<ISettingService>(new SettingService(resolver.GetRequiredService<IDialogueService>()));

        // Localization Service
        services.RegisterConstant<ILocalizationService>(
            new LocalizationService(resolver.GetRequiredService<ISettingService>()));

        // Download Window View Model
        services.RegisterLazySingleton<IDownloadWindowViewModel>(() => new DownloadWindowViewModel(
            resolver.GetRequiredService<IDialogueService>(),
            resolver.GetRequiredService<IGitHubService>(),
            resolver.GetRequiredService<ISettingService>()));

        // Local Service
        services.RegisterConstant<ILocalService>(new LocalService(
            resolver.GetRequiredService<IDialogueService>(),
            resolver.GetRequiredService<ISettingService>(),
            resolver.GetRequiredService<IDownloadWindowViewModel>()));

        // Mod Service
        services.RegisterConstant<IModService>(new ModService(
            resolver.GetRequiredService<IDialogueService>(),
            resolver.GetRequiredService<IGitHubService>(),
            resolver.GetRequiredService<ISettingService>(),
            resolver.GetRequiredService<ILocalService>()));

        // Mod Manage UserControl View Model
        services.RegisterLazySingleton<IModManageViewModel>(() => new ModManageViewModel(
            resolver.GetRequiredService<IGitHubService>(),
            resolver.GetRequiredService<ISettingService>(),
            resolver.GetRequiredService<ILocalService>(),
            resolver.GetRequiredService<IModService>()));

        // Settings UserControl View Model
        services.RegisterLazySingleton<ISettingsViewModel>(() => new SettingsViewModel(
            resolver.GetRequiredService<ISettingService>(),
            resolver.GetRequiredService<IModManageViewModel>()));

        // Main Window View Model
        services.RegisterLazySingleton<IMainWindowViewModel>(() => new MainWindowViewModel(
            resolver.GetRequiredService<IModManageViewModel>()));
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