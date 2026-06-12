namespace Euterpe.Core;

internal sealed class ChartingToolStep : ISetupStep
{
    #region Injections

    public required ILogger<ChartingToolStep> Logger { get; init; }

    #endregion Injections

    public SetupOptionKinds Kinds => SetupOptionKinds.ChartingTool;

    public Task ExecuteAsync(IProgress<string>? progress = null, CancellationToken cancellationToken = default)
    {
        Logger.ZLogWarning($"SetupStep '{Kinds}' not implemented yet");
        return Task.CompletedTask;
    }
}
