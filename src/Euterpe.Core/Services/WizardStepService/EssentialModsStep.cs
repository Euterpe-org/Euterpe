namespace Euterpe.Core;

internal sealed class EssentialModsStep : IWizardStep
{
    public WizardOptionKinds Kinds => WizardOptionKinds.EssentialMods;

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        await ModManageService.InitializeModsAsync().ConfigureAwait(false);
        Logger.ZLogWarning($"WizardStep '{Kinds}' not implemented yet");
    }

    #region Injections

    [UsedImplicitly]
    public required IModManageService ModManageService { get; init; }

    [UsedImplicitly]
    public required ILogger<EssentialModsStep> Logger { get; init; }

    #endregion Injections
}