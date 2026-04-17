namespace Euterpe.Core;

internal sealed class ChartingToolStep : IWizardStep
{
    #region Injections

    [UsedImplicitly]
    public required ILogger<ChartingToolStep> Logger { get; init; }

    #endregion Injections

    public string Name => "ChartingTool";

    public Task ExecuteAsync(IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        Logger.ZLogWarning($"WizardStep '{Name}' not implemented yet");
        return Task.CompletedTask;
    }
}