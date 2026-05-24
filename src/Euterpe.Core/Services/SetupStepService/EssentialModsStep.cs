namespace Euterpe.Core;

internal sealed class EssentialModsStep : ISetupStep
{
    public SetupOptionKinds Kinds => SetupOptionKinds.EssentialMods;

    public async Task ExecuteAsync(IProgress<string>? progress = null, CancellationToken cancellationToken = default)
    {
        progress?.Report("Initializing essential mods ...");
        await ModManageService.InitializeModsAsync().ConfigureAwait(false);
        Logger.ZLogWarning($"SetupStep '{Kinds}' not implemented yet");
    }

    #region Injections

    public required IModManageService ModManageService { get; init; }
    public required ILogger<EssentialModsStep> Logger { get; init; }

    #endregion Injections
}