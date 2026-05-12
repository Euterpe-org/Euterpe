namespace Euterpe.ViewModels.Pages;

public sealed partial class HomePageViewModel
{
    private void StartBackgroundTasks()
    {
        BindAccountAsync().SafeFireAndForget();
        CheckModdingDependenciesAsync().SafeFireAndForget(ex => Logger.ZLogError(ex, $"Failed to check modding dependencies"));
    }

    private async Task BindAccountAsync()
    {
        var request = await UidProvider.GetMuseDashUidRequestAsync().ConfigureAwait(false);
        if (request is null)
        {
            Logger.ZLogWarning($"Failed to get MuseDash user ID. Skipping account binding.");
            return;
        }

        try
        {
            await AccountClient.BindVanillaAccountAsync(request).ConfigureAwait(false);
            Logger.ZLogInformation($"Successfully bound MuseDash account.");
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to bind MuseDash account.");
        }
    }

    private async Task CheckModdingDependenciesAsync()
    {
        await CheckDotNetRuntimeAsync().ConfigureAwait(true);
        await CheckMelonLoaderAsync().ConfigureAwait(true);
    }

    private async Task CheckDotNetRuntimeAsync()
    {
        if (await RuntimeInstaller.CheckInstalledAsync().ConfigureAwait(true))
        {
            return;
        }

        Logger.ZLogInformation($".NET runtime not installed, opening repair dialog");
        await SetupDialogService.ShowOptionRepairAsync(SetupOptionKinds.DotNetRuntime).ConfigureAwait(false);
    }

    private async Task CheckMelonLoaderAsync()
    {
        if (GameConfig.MelonLoaderSemVersion is not null)
        {
            return;
        }

        Logger.ZLogInformation($"MelonLoader not installed, opening repair dialog");
        await SetupDialogService.ShowOptionRepairAsync(SetupOptionKinds.MelonLoader).ConfigureAwait(false);
    }
}