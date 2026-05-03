namespace Euterpe.Core;

internal sealed class DotNetRuntimeStep : IWizardStep
{
    #region Injections

    [UsedImplicitly]
    public required IPlatformService PlatformService { get; init; }

    #endregion Injections

    public WizardOptionKinds Kinds => WizardOptionKinds.DotNetRuntime;

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (await PlatformService.CheckDotNetRuntimeInstalledAsync().ConfigureAwait(false))
        {
            return;
        }

        await PlatformService.InstallDotNetRuntimeAsync().ConfigureAwait(false);
    }
}