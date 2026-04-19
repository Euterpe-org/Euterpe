namespace Euterpe.Core;

internal sealed class UninstallConflictsStep : IWizardStep
{
    public WizardOptionKinds Kinds => WizardOptionKinds.UninstallConflicts;

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        await ModManageService.InitializeModsAsync().ConfigureAwait(false);

        var mod = ModManageService.FindModByName(AppName);
        if (mod is null)
        {
            return;
        }

        foreach (var modName in mod.IncompatibleMods)
        {
            var incompatibleMod = ModManageService.FindModByName(modName);
            if (incompatibleMod is null)
            {
                continue;
            }

            Logger.ZLogInformation($"Conflict detected: {modName} is installed and will be uninstalled");
            await ModManageService.UninstallModAsync(incompatibleMod).ConfigureAwait(false);
        }
    }

    #region Injections

    [UsedImplicitly]
    public required IModManageService ModManageService { get; init; }

    [UsedImplicitly]
    public required ILogger<UninstallConflictsStep> Logger { get; init; }

    #endregion Injections
}