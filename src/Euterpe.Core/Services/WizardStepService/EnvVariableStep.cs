namespace Euterpe.Core;

internal sealed class EnvVariableStep : IWizardStep
{
    #region Injections

    [UsedImplicitly]
    public required IPlatformService PlatformService { get; init; }

    #endregion Injections

    public string Name => "EnvVariable";

    public async Task ExecuteAsync(IProgress<double>? progress = null, CancellationToken cancellationToken = default) =>
        PlatformService.SetPathEnvironmentVariable();
}