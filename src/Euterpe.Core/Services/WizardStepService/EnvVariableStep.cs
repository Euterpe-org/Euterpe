namespace Euterpe.Core;

internal sealed class EnvVariableStep : IWizardStep
{
    #region Injections

    [UsedImplicitly]
    public required IPlatformService PlatformService { get; init; }

    #endregion Injections

    public WizardOptionKinds Kinds => WizardOptionKinds.EnvVariable;

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (PlatformService.CheckPathEnvironmentVariableSet())
        {
            return;
        }

        PlatformService.SetPathEnvironmentVariable();
    }
}