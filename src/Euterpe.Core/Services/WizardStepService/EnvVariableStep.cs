namespace Euterpe.Core;

internal sealed class EnvVariableStep : IWizardStep
{
    #region Injections

    [UsedImplicitly]
    public required IGamePathEnvironment PathEnvironment { get; init; }

    #endregion Injections

    public WizardOptionKinds Kinds => WizardOptionKinds.EnvVariable;

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (PathEnvironment.IsSet())
        {
            return;
        }

        PathEnvironment.Set();
    }
}