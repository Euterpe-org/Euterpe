namespace Euterpe.Core;

internal sealed class MelonLoaderStep : IWizardStep
{
    #region Injections

    [UsedImplicitly]
    public required ILogger<MelonLoaderStep> Logger { get; init; }

    #endregion Injections

    public string Name => "MelonLoader";

    public Task ExecuteAsync(IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        Logger.ZLogWarning($"WizardStep '{Name}' not implemented yet");
        return Task.CompletedTask;
    }
}