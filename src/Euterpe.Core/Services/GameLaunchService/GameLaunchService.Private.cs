using CliWrap;

namespace Euterpe.Core;

internal sealed partial class GameLaunchService
{
    private async Task LaunchGameAsync(string gameId, params IEnumerable<string> launchArguments)
    {
        await Cli.Wrap(Config.SteamExecPath)
            .WithArguments(args =>
                {
                    args.Add("-applaunch");
                    args.Add(gameId);
                    args.Add(launchArguments);
                }
            )
            .WithValidation(CommandResultValidation.None)
            .ExecuteAsync()
            .ConfigureAwait(false);

        Logger.LogInformation($"Launching game {gameId} with launch arguments: {string.Join(' ', launchArguments)}");
    }
}
