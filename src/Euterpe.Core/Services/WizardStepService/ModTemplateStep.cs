namespace Euterpe.Core;

internal sealed class ModTemplateStep : IWizardStep
{
    #region Injections

    [UsedImplicitly]
    public required IPlatformService PlatformService { get; init; }

    #endregion Injections

    public string Name => "ModTemplate";

    public async Task ExecuteAsync(IProgress<double>? progress = null, CancellationToken cancellationToken = default) =>
        await PlatformService.InstallModTemplateAsync().ConfigureAwait(false);
}