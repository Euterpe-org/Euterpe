namespace Euterpe.Core;

internal sealed class DotNetRuntimeStep : ISetupStep
{
    public SetupOptionKinds Kinds => SetupOptionKinds.DotNetRuntime;

    public async Task ExecuteAsync(IProgress<string> progress, CancellationToken cancellationToken = default)
    {
        var release = await DependencyAcquireService.GetLatestMelonLoaderReleaseAsync(cancellationToken).ConfigureAwait(false);

        if (await RuntimeInstaller.CheckInstalledAsync(release.DotNetRuntimeVersion).ConfigureAwait(false))
        {
            return;
        }

        progress.Report(XAML.Setup_Progress_InstallingDotNetRuntime);

        await RuntimeInstaller.InstallAsync(release.DotNetRuntimeVersion).ConfigureAwait(false);
    }

    #region Injections

    public required IDependencyAcquireService DependencyAcquireService { get; init; }
    public required IGameRuntimeInstaller RuntimeInstaller { get; init; }

    #endregion Injections
}
