namespace Euterpe.Core;

internal sealed class MelonLoaderStep : IWizardStep
{
    public WizardTaskKind Kind => WizardTaskKind.MelonLoader;

    public async Task ExecuteAsync(IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        await DependencyAcquireService.AcquireForMelonLoaderAsync(progress: progress, cancellationToken: cancellationToken).ConfigureAwait(false);
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