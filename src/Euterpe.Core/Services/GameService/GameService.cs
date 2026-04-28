namespace Euterpe.Core;

internal sealed partial class GameService : IGameService
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

        await LaunchGameAsync(GameConfig.SteamAppId, launchArguments).ConfigureAwait(false);
    }

    public async Task LaunchVanillaGameAsync()
    {
        const string launchArguments = "--no-mods";
        await LaunchGameAsync(GameConfig.SteamAppId, launchArguments).ConfigureAwait(false);
    }

    #region Injections

    [UsedImplicitly]
    public required Config Config { get; init; }

    [UsedImplicitly]
    public required GameConfig GameConfig { get; init; }

    [UsedImplicitly]
    public required ILogger<GameService> Logger { get; init; }

    [UsedImplicitly]
    public required IPlatformService PlatformService { get; init; }

    #endregion Injections
}