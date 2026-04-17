namespace Euterpe.Core;

internal sealed class EssentialModsStep : IWizardStep
{
    public WizardTaskKind Kind => WizardTaskKind.EssentialMods;

    public async Task ExecuteAsync(IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        await ModManageService.InitializeModsAsync().ConfigureAwait(false);
        Logger.ZLogWarning($"WizardStep '{Kind}' not implemented yet");
    }

    #region Injections

    [UsedImplicitly]
    public required IModManageService ModManageService { get; init; }

    [UsedImplicitly]
    public required ILogger<EssentialModsStep> Logger { get; init; }

    #endregion Injections
}