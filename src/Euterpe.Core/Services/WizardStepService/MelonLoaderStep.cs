namespace Euterpe.Core;

internal sealed class MelonLoaderStep : IWizardStep
{
    public WizardOptionKinds Kinds => WizardOptionKinds.MelonLoader;

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        GameLocalService.ReadMelonLoaderVersion();
        if (GameConfig.MelonLoaderVersion is not null)
        {
            return;
        }

        await DependencyAcquireService.AcquireForMelonLoaderAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        await GameLocalService.InstallMelonLoaderAsync().ConfigureAwait(false);
    }

    #region Injections

    [UsedImplicitly]
    public required GameConfig GameConfig { get; init; }

    [UsedImplicitly]
    public required IDependencyAcquireService DependencyAcquireService { get; init; }

    [UsedImplicitly]
    public required IGameLocalService GameLocalService { get; init; }

    #endregion Injections
}