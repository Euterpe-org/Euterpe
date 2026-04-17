namespace Euterpe.Core;

internal sealed class DotNetSdkStep : IWizardStep
{
    #region Injections

    [UsedImplicitly]
    public required IPlatformService PlatformService { get; init; }

    #endregion Injections

    public string Name => "DotNetSdk";

    public async Task ExecuteAsync(IProgress<double>? progress = null, CancellationToken cancellationToken = default) =>
        await PlatformService.InstallDotNetSdkAsync().ConfigureAwait(false);
}