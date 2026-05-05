namespace Euterpe.Core;

internal sealed class MelonLoaderStep : IWizardStep
{
    public WizardOptionKinds Kinds => WizardOptionKinds.MelonLoader;

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        await DependencyAcquireService.AcquireForMelonLoaderAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        await GameLocalService.InstallMelonLoaderAsync().ConfigureAwait(false);
        GameLocalService.ReadMelonLoaderVersion();
    }

    #region Injections

    [UsedImplicitly]
    public required IDependencyAcquireService DependencyAcquireService { get; init; }

    [UsedImplicitly]
    public required IGameLocalService GameLocalService { get; init; }

    #endregion Injections
}