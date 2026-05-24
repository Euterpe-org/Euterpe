namespace Euterpe.Core;

internal sealed class DotNetRuntimeStep : ISetupStep
{
    #region Injections

    public required IGameRuntimeInstaller RuntimeInstaller { get; init; }

    #endregion Injections

    public SetupOptionKinds Kinds => SetupOptionKinds.DotNetRuntime;

    public async Task ExecuteAsync(IProgress<string>? progress = null, CancellationToken cancellationToken = default)
    {
        if (await RuntimeInstaller.CheckInstalledAsync().ConfigureAwait(false))
        {
            return;
        }

        progress?.Report("Installing .NET runtime ...");
        await RuntimeInstaller.InstallAsync().ConfigureAwait(false);
    }
}