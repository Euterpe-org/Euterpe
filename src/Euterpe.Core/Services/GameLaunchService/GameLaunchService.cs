namespace Euterpe.Core;

internal sealed partial class GameLaunchService : IGameLaunchService
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

    public required Config Config { get; init; }
    public required GameConfig GameConfig { get; init; }
    public required ILogger<GameLaunchService> Logger { get; init; }

    #endregion Injections
}