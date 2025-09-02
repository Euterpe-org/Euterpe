namespace MuseDashModTools.Core;

internal sealed class GameService : IGameService
{
    public async Task LaunchModdedGameAsync()
    {
        var launchArguments = new List<string>();
        if (!Config.ShowConsole)
        {
            launchArguments.Add("--melonloader.hideconsole");
        }

        if (!Config.ShowStartScreen)
        {
            launchArguments.Add("--melonloader.disablestartscreen");
        }

        await PlatformService.LaunchGameWithArgsAsync(MuseDashGameId, launchArguments).ConfigureAwait(false);
    }

    public async Task LaunchVanillaGameAsync()
    {
        const string launchArguments = "--no-mods";
        await PlatformService.LaunchGameWithArgAsync(MuseDashGameId, launchArguments).ConfigureAwait(false);
    }

    #region Injections

    [UsedImplicitly]
    public required Config Config { get; init; }

    [UsedImplicitly]
    public required ILogger<GameService> Logger { get; init; }

    [UsedImplicitly]
    public required IPlatformService PlatformService { get; init; }

    #endregion Injections
}