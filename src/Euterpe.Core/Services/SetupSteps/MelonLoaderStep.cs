namespace Euterpe.Core;

internal sealed class MelonLoaderStep : ISetupStep
{
    public SetupOptionKinds Kinds => SetupOptionKinds.MelonLoader;

    public async Task ExecuteAsync(IProgress<string> progress, CancellationToken cancellationToken = default)
    {
        progress.Report(XAML.Setup_Progress_InstallingMelonLoader);

        await GameLocalService.UninstallMelonLoaderAsync().ConfigureAwait(false);
        await DependencyAcquireService.AcquireForMelonLoaderAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        await GameLocalService.InstallMelonLoaderAsync().ConfigureAwait(false);
        GameLocalService.ReadMelonLoaderVersion();
    }

    #region Injections

    public required IDependencyAcquireService DependencyAcquireService { get; init; }
    public required IGameLocalService GameLocalService { get; init; }

    #endregion Injections
}
