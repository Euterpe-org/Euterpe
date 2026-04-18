namespace Euterpe.Core;

internal sealed class MelonLoaderStep : IWizardStep
{
    public WizardOptionKinds Kinds => WizardOptionKinds.MelonLoader;

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        await DependencyAcquireService.AcquireForMelonLoaderAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        await LocalService.InstallMelonLoaderAsync().ConfigureAwait(false);
        LocalService.ReadMelonLoaderVersion();
    }

    #region Injections

    [UsedImplicitly]
    public required IDependencyAcquireService DependencyAcquireService { get; init; }

    [UsedImplicitly]
    public required ILocalService LocalService { get; init; }

    #endregion Injections
}