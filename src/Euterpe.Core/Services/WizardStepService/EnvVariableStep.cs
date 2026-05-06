namespace Euterpe.Core;

internal sealed class EnvVariableStep : IWizardStep
{
    #region Injections

    [UsedImplicitly]
    public required IGamePathEnvironment PathEnvironment { get; init; }

    #endregion Injections

    public WizardOptionKinds Kinds => WizardOptionKinds.EnvVariable;

    public async Task ExecuteAsync(IProgress<string>? progress = null, CancellationToken cancellationToken = default)
    {
        if (PathEnvironment.IsSet())
        {
            return;
        }

        progress?.Report("Setting environment variable ...");
        PathEnvironment.Set();
    }
}