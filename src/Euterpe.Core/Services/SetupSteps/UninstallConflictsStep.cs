namespace Euterpe.Core;

internal sealed class UninstallConflictsStep : ISetupStep
{
    public SetupOptionKinds Kinds => SetupOptionKinds.UninstallConflicts;

    public async Task ExecuteAsync(IProgress<string>? progress = null, CancellationToken cancellationToken = default)
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
            if (incompatibleMod is not { IsLocal: true })
            {
                continue;
            }

            Logger.ZLogInformation($"Conflict detected: {modName} is installed and will be uninstalled");
            progress?.Report($"Uninstalling incompatible mod: {modName} ...");
            await ModManageService.UninstallModAsync(incompatibleMod).ConfigureAwait(false);
        }
    }

    #region Injections

    public required IModManageService ModManageService { get; init; }
    public required ILogger<UninstallConflictsStep> Logger { get; init; }

    #endregion Injections
}
