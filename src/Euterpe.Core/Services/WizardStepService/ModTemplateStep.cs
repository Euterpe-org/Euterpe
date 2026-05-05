namespace Euterpe.Core;

internal sealed class ModTemplateStep : IWizardStep
{
    #region Injections

    [UsedImplicitly]
    public required IGameModTemplateInstaller ModTemplateInstaller { get; init; }

    #endregion Injections

    public WizardOptionKinds Kinds => WizardOptionKinds.ModTemplate;

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (await ModTemplateInstaller.CheckInstalledAsync().ConfigureAwait(false))
        {
            return;
        }

        await ModTemplateInstaller.InstallAsync().ConfigureAwait(false);
    }
}