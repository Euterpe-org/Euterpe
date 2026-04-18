namespace Euterpe.Core;

internal sealed class DotNetSdkStep : IWizardStep
{
    #region Injections

    [UsedImplicitly]
    public required IPlatformService PlatformService { get; init; }

    #endregion Injections

    public WizardOptionKinds Kinds => WizardOptionKinds.DotNetSdk;

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (await PlatformService.CheckDotNetSdkInstalledAsync().ConfigureAwait(false))
        {
            return;
        }

        await PlatformService.InstallDotNetSdkAsync().ConfigureAwait(false);
    }
}