namespace Euterpe.Core;

internal sealed class ChartingToolStep : IWizardStep
{
    #region Injections

    [UsedImplicitly]
    public required ILogger<ChartingToolStep> Logger { get; init; }

    #endregion Injections

    public WizardOptionKinds Kinds => WizardOptionKinds.ChartingTool;

    public Task ExecuteAsync(IProgress<string>? progress = null, CancellationToken cancellationToken = default)
    {
        Logger.ZLogWarning($"WizardStep '{Kinds}' not implemented yet");
        return Task.CompletedTask;
    }
}