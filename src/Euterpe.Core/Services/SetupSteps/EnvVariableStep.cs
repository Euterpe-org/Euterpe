namespace Euterpe.Core;

internal sealed class EnvVariableStep : ISetupStep
{
    #region Injections

    public required IGamePathEnvironment PathEnvironment { get; init; }

    #endregion Injections

    public SetupOptionKinds Kinds => SetupOptionKinds.EnvVariable;

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
