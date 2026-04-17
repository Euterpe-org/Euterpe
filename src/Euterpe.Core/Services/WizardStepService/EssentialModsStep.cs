namespace Euterpe.Core;

internal sealed class EssentialModsStep : IWizardStep
{
    public string Name => "EssentialMods";

    public Task ExecuteAsync(IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        Logger.ZLogWarning($"WizardStep '{Name}' not implemented yet");
        return Task.CompletedTask;
    }

    #region Injections

    [UsedImplicitly]
    public required IModManageService ModManageService { get; init; }

    [UsedImplicitly]
    public required ILogger<EssentialModsStep> Logger { get; init; }

    #endregion Injections
}