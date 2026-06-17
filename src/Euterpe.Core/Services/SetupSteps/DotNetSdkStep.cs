namespace Euterpe.Core;

internal sealed class DotNetSdkStep : ISetupStep
{
    #region Injections

    public required IDotNetSdkInstaller SdkInstaller { get; init; }

    #endregion Injections

    public SetupOptionKinds Kinds => SetupOptionKinds.DotNetSdk;

    public async Task ExecuteAsync(IProgress<string> progress, CancellationToken cancellationToken = default)
    {
        if (await SdkInstaller.CheckInstalledAsync().ConfigureAwait(false))
        {
            return;
        }

        progress.Report(XAML.Setup_Progress_InstallingDotNetSdk);
        await SdkInstaller.InstallAsync().ConfigureAwait(false);
    }
}
