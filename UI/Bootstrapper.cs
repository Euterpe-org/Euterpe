using System;
using System.Net.Http;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Contracts.ViewModels;
using MuseDashModToolsUI.Models;
using MuseDashModToolsUI.Services;
using MuseDashModToolsUI.ViewModels;
using Splat;

namespace MuseDashModToolsUI;

public static class Bootstrapper
{
    public static void Register(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
    {
        services.RegisterConstant<ISettings>(new Settings());
        services.Register<IDialogueService>(() => new DialogueService());
        services.Register<IGitHubService>(() => new GitHubService(new HttpClient(), resolver.GetRequiredService<IDialogueService>()));
        services.Register<ILocalService>(() => new LocalService());
        services.RegisterLazySingleton<IDownloadWindowViewModel>(() => new DownloadWindowViewModel(
            resolver.GetRequiredService<ISettings>(),
            resolver.GetRequiredService<IGitHubService>(),
            resolver.GetRequiredService<IDialogueService>()));
        services.RegisterLazySingleton<IMainWindowViewModel>(() => new MainWindowViewModel(
            resolver.GetRequiredService<ISettings>(),
            resolver.GetRequiredService<IGitHubService>(),
            resolver.GetRequiredService<ILocalService>(),
            resolver.GetRequiredService<IDialogueService>(),
            resolver.GetRequiredService<IDownloadWindowViewModel>()));
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