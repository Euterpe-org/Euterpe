namespace Euterpe.Core;

internal sealed class EssentialModsStep : ISetupStep
{
    public SetupOptionKinds Kinds => SetupOptionKinds.EssentialMods;

    public async Task ExecuteAsync(IProgress<string> progress, CancellationToken cancellationToken = default)
    {
        progress.Report("Initializing essential mods ...");
        await ModManageService.InitializeModsAsync().ConfigureAwait(false);

        var mod = ModManageService.FindModByName(AppName);
        if (mod is null)
        {
            Logger.ZLogWarning($"Essential mod '{AppName}' not found");
            return;
        }

        await ModManageService.InstallModAsync(mod).ConfigureAwait(false);
        progress.Report("Essential mods installed");
    }

    #region Injections

    public required IModManageService ModManageService { get; init; }
    public required ILogger<EssentialModsStep> Logger { get; init; }

    #endregion Injections
}
