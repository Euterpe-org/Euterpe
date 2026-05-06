namespace Euterpe.Core;

internal sealed class DotNetSdkStep : IWizardStep
{
    #region Injections

    [UsedImplicitly]
    public required IDotNetSdkInstaller SdkInstaller { get; init; }

    #endregion Injections

    public WizardOptionKinds Kinds => WizardOptionKinds.DotNetSdk;

    public async Task ExecuteAsync(IProgress<string>? progress = null, CancellationToken cancellationToken = default)
    {
        if (await SdkInstaller.CheckInstalledAsync().ConfigureAwait(false))
        {
            return;
        }

        progress?.Report("Installing .NET SDK ...");
        await SdkInstaller.InstallAsync().ConfigureAwait(false);
    }
}