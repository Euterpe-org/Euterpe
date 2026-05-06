namespace Euterpe.Core;

internal sealed class DotNetRuntimeStep : IWizardStep
{
    #region Injections

    [UsedImplicitly]
    public required IGameRuntimeInstaller RuntimeInstaller { get; init; }

    #endregion Injections

    public WizardOptionKinds Kinds => WizardOptionKinds.DotNetRuntime;

    public async Task ExecuteAsync(IProgress<string>? progress = null, CancellationToken cancellationToken = default)
    {
        if (await RuntimeInstaller.CheckInstalledAsync().ConfigureAwait(false))
        {
            return;
        }

        progress?.Report("Installing .NET runtime ...");
        await RuntimeInstaller.InstallAsync().ConfigureAwait(false);
    }
}