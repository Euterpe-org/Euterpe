namespace Euterpe.Core;

internal sealed class UninstallConflictsStep : IWizardStep
{
    private static readonly string[] ConflictModNames =
    [
        "CustomAlbums",
        "Headquarters",
        "Cinema",
        "CinemaToggler",
        "CustomAnchorSupport"
    ];

    public WizardOptionKinds Kinds => WizardOptionKinds.UninstallConflicts;

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        await ModManageService.InitializeModsAsync().ConfigureAwait(false);

        foreach (var modName in ConflictModNames)
        {
            var mod = ModManageService.FindModByName(modName);
            if (mod is null)
            {
                continue;
            }

            Logger.ZLogInformation($"Conflict detected: {modName} is installed and will be uninstalled");
            await ModManageService.UninstallModAsync(mod).ConfigureAwait(false);
        }
    }

    #region Injections

    [UsedImplicitly]
    public required IModManageService ModManageService { get; init; }

    [UsedImplicitly]
    public required ILogger<UninstallConflictsStep> Logger { get; init; }

    #endregion Injections
}