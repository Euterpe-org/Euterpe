using Semver;

namespace Euterpe.Features.Home;

public sealed partial class HomePageViewModel
{
    private void StartBackgroundTasks()
    {
        CheckModdingDependenciesAsync().SafeFireAndForget(ex => Logger.LogError(ex, "Failed to check modding dependencies"));
        UpdateChartsAsync().SafeFireAndForget(ex => Logger.LogError(ex, "Failed to auto-update charts"));
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

        Logger.LogInformation(".NET runtime not installed, opening repair dialog");
        await SetupDialogService.ShowOptionRepairAsync(SetupOptionKinds.DotNetRuntime).ConfigureAwait(false);
    }

    private async Task CheckMelonLoaderAsync()
    {
        if (GameConfig.MelonLoaderSemVersion is not { } installedVersion)
        {
            Logger.LogInformation("MelonLoader not installed, opening repair dialog");
            await SetupDialogService.ShowOptionRepairAsync(SetupOptionKinds.MelonLoader).ConfigureAwait(false);
            return;
        }

        var latestRaw = await DependencyAcquireService.GetLatestMelonLoaderVersionAsync().ConfigureAwait(true);
        var latestVersion = SemVersion.Parse(latestRaw);

        if (installedVersion.ComparePrecedenceTo(latestVersion) >= 0)
        {
            return;
        }

        Logger.LogInformation("MelonLoader outdated ({InstalledVersion} < {LatestVersion}), opening repair dialog", installedVersion, latestVersion);
        await SetupDialogService.ShowOptionRepairAsync(SetupOptionKinds.MelonLoader).ConfigureAwait(false);
    }

    private async Task UpdateChartsAsync()
    {
        await ChartManageService.InitializeChartsAsync().ConfigureAwait(false);
        await ChartManageService.UpdateAllChartsAsync().ConfigureAwait(false);
    }
}
