namespace Euterpe.Core;

internal sealed class ModTemplateStep : ISetupStep
{
    #region Injections

    [UsedImplicitly]
    public required IGameModTemplateInstaller ModTemplateInstaller { get; init; }

    #endregion Injections

    public SetupOptionKinds Kinds => SetupOptionKinds.ModTemplate;

    public async Task ExecuteAsync(IProgress<string>? progress = null, CancellationToken cancellationToken = default)
    {
        if (await ModTemplateInstaller.CheckInstalledAsync().ConfigureAwait(false))
        {
            return;
        }

        progress?.Report("Installing mod template ...");
        await ModTemplateInstaller.InstallAsync().ConfigureAwait(false);
    }
}