using System.Text;

namespace MuseDashModTools.Core;

internal sealed partial class GameService : IGameService
{
    public void LaunchModdedGame()
    {
        var launchArguments = new StringBuilder();
        if (!Config.ShowConsole)
        {
            launchArguments.Append("//--melonloader.hideconsole");
        }

        LaunchGame(launchArguments.ToString());
    }

    public void LaunchVanillaGame()
    {
        var launchArguments = new StringBuilder();
        launchArguments.Append("//--no-mods");

        LaunchGame(launchArguments.ToString());
    }

    #region Injections

    [UsedImplicitly]
    public required Config Config { get; init; }

    [UsedImplicitly]
    public required ILogger<GameService> Logger { get; init; }

    #endregion Injections
}