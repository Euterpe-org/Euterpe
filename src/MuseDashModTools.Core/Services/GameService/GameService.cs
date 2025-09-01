using System.Text;

namespace MuseDashModTools.Core;

internal sealed class GameService : IGameService
{
    public async Task LaunchModdedGameAsync()
    {
        var launchArguments = new StringBuilder();
        if (!Config.ShowConsole)
        {
            launchArguments.Append("--melonloader.hideconsole");
        }

        await PlatformService.LaunchGameWithArgsAsync(MuseDashGameId, launchArguments.ToString()).ConfigureAwait(false);
    }

    public async Task LaunchVanillaGameAsync()
    {
        var launchArguments = new StringBuilder();
        launchArguments.Append("--no-mods");

        await PlatformService.LaunchGameWithArgsAsync(MuseDashGameId, launchArguments.ToString()).ConfigureAwait(false);
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