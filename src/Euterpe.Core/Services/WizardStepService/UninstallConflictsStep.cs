namespace Euterpe.Core;

internal sealed class UninstallConflictsStep : IWizardStep
{
    public WizardOptionKinds Kinds => WizardOptionKinds.UninstallConflicts;

    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        Logger.ZLogWarning($"WizardStep '{Kinds}' not implemented yet");
        return Task.CompletedTask;
    }

    #region Injections

    [UsedImplicitly]
    public required IModManageService ModManageService { get; init; }

    [UsedImplicitly]
    public required ILogger<UninstallConflictsStep> Logger { get; init; }

    #endregion Injections
}