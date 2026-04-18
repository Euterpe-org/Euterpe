namespace Euterpe.Core;

internal sealed class ModTemplateStep : IWizardStep
{
    #region Injections

    [UsedImplicitly]
    public required IPlatformService PlatformService { get; init; }

    #endregion Injections

    public WizardOptionKinds Kinds => WizardOptionKinds.ModTemplate;

    public async Task ExecuteAsync(CancellationToken cancellationToken = default) =>
        await PlatformService.InstallModTemplateAsync().ConfigureAwait(false);
}