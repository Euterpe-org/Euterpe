namespace Euterpe.Core;

internal sealed class MelonLoaderStep : IWizardStep
{
    public WizardOptionKinds Kinds => WizardOptionKinds.MelonLoader;

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        GameLocalService.ReadMelonLoaderVersion();
        var localVersion = GameConfig.MelonLoaderSemVersion;

        var latestRaw = await DependencyAcquireService.GetLatestMelonLoaderVersionAsync(cancellationToken).ConfigureAwait(false);
        if (!SemVersion.TryParse(latestRaw, out var latestVersion))
        {
            if (localVersion is null)
            {
                throw new InvalidOperationException("Failed to fetch latest MelonLoader version.");
            }

            Logger.ZLogWarning($"Failed to fetch latest MelonLoader version, keeping installed {localVersion}");
            return;
        }

        if (localVersion?.ComparePrecedenceTo(latestVersion) >= 0)
        {
            return;
        }

        if (localVersion is not null)
        {
            Logger.ZLogInformation($"MelonLoader outdated: {localVersion} < {latestVersion}, upgrading");
        }

        await DependencyAcquireService.AcquireForMelonLoaderAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        await GameLocalService.InstallMelonLoaderAsync().ConfigureAwait(false);
        GameLocalService.ReadMelonLoaderVersion();
    }

    #region Injections

    [UsedImplicitly]
    public required GameConfig GameConfig { get; init; }

    [UsedImplicitly]
    public required IDependencyAcquireService DependencyAcquireService { get; init; }

    [UsedImplicitly]
    public required IGameLocalService GameLocalService { get; init; }

    [UsedImplicitly]
    public required ILogger<MelonLoaderStep> Logger { get; init; }

    #endregion Injections
}