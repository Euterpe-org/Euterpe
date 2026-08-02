namespace Euterpe.Core;

internal sealed class ChartingToolStep : ISetupStep
{
    #region Injections

    public required ILogger<ChartingToolStep> Logger { get; init; }

    #endregion Injections

    public SetupOptionKinds Kinds => SetupOptionKinds.ChartingTool;

    public Task ExecuteAsync(IProgress<string> progress, CancellationToken cancellationToken = default)
    {
        Logger.LogWarning("SetupStep '{Kinds}' not implemented yet", Kinds);
        return Task.CompletedTask;
    }
}
