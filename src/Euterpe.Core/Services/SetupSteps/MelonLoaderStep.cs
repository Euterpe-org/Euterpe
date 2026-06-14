namespace Euterpe.Core;

internal sealed class MelonLoaderStep : ISetupStep
{
    public SetupOptionKinds Kinds => SetupOptionKinds.MelonLoader;

    public async Task ExecuteAsync(IProgress<string> progress, CancellationToken cancellationToken = default)
    {
        GameLocalService.ReadMelonLoaderVersion();

        if (GameConfig.MelonLoaderSemVersion is not { } localVersion)
        {
            progress.Report("Installing MelonLoader ...");
            await InstallAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        var latestRaw = await DependencyAcquireService.GetLatestMelonLoaderVersionAsync(cancellationToken).ConfigureAwait(false);
        if (!SemVersion.TryParse(latestRaw, out var latestVersion))
        {
            throw new InvalidOperationException($"Failed to fetch latest MelonLoader version, got invalid version string: {latestRaw}");
        }

        if (localVersion.ComparePrecedenceTo(latestVersion) >= 0)
        {
            return;
        }

        Logger.ZLogInformation($"MelonLoader outdated: {localVersion} < {latestVersion}, upgrading");
        progress.Report($"Upgrading MelonLoader {localVersion} → {latestVersion} ...");
        await InstallAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task InstallAsync(CancellationToken cancellationToken)
    {
        await DependencyAcquireService.AcquireForMelonLoaderAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        await GameLocalService.InstallMelonLoaderAsync().ConfigureAwait(false);
        GameLocalService.ReadMelonLoaderVersion();
    }

    #region Injections

    public required GameConfig GameConfig { get; init; }
    public required IDependencyAcquireService DependencyAcquireService { get; init; }
    public required IGameLocalService GameLocalService { get; init; }
    public required ILogger<MelonLoaderStep> Logger { get; init; }

    #endregion Injections
}
