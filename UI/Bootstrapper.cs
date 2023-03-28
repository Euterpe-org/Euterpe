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
        services.Register<IDialogueService>(() => new DialogueService());
        services.Register<IGitHubService>(() => new GitHubService(new HttpClient(), resolver.GetRequiredService<IDialogueService>()));
        services.RegisterConstant<ISettingService>(new SettingService(resolver.GetRequiredService<IDialogueService>()));
        services.RegisterLazySingleton<IDownloadWindowViewModel>(() => new DownloadWindowViewModel(
            resolver.GetRequiredService<IDialogueService>(),
            resolver.GetRequiredService<IGitHubService>(),
            resolver.GetRequiredService<ISettingService>()));
        services.RegisterConstant<ILocalService>(new LocalService(
            resolver.GetRequiredService<IDialogueService>(),
            resolver.GetRequiredService<ISettingService>(),
            resolver.GetRequiredService<IDownloadWindowViewModel>()));
        services.RegisterConstant<IModService>(new ModService(
            resolver.GetRequiredService<IDialogueService>(),
            resolver.GetRequiredService<IGitHubService>(),
            resolver.GetRequiredService<ISettingService>(),
            resolver.GetRequiredService<ILocalService>()));
        services.RegisterLazySingleton<IMainWindowViewModel>(() => new MainWindowViewModel(
            resolver.GetRequiredService<ISettingService>(),
            resolver.GetRequiredService<ILocalService>(),
            resolver.GetRequiredService<IModService>()));
    }

    public static TService GetRequiredService<TService>(this IReadonlyDependencyResolver resolver)
    {
        var service = resolver.GetService<TService>();
        if (service is null) // Splat is not able to resolve type for us
        {
            throw new InvalidOperationException($"Failed to resolve object of type {typeof(TService)}"); // throw error with detailed description
        }

        return service; // return instance if not null
    }
}