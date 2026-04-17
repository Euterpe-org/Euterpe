namespace Euterpe.Core;

internal sealed class UninstallConflictsStep : IWizardStep
{
    public string Name => "UninstallConflicts";

    public Task ExecuteAsync(IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        Logger.ZLogWarning($"WizardStep '{Name}' not implemented yet");
        return Task.CompletedTask;
    }

    #region Injections

    [UsedImplicitly]
    public required IModManageService ModManageService { get; init; }

    [UsedImplicitly]
    public required ILogger<UninstallConflictsStep> Logger { get; init; }

    #endregion Injections
}