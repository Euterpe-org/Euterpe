using CliWrap;

namespace MuseDashModTools.Core;

internal sealed partial class GameService
{
    private Task<bool> LaunchGameWithArgAsync(string gameId, string launchArgument) =>
        LaunchGameWithArgsAsync(gameId, [launchArgument]);

    private async Task<bool> LaunchGameWithArgsAsync(string gameId, IEnumerable<string> launchArguments)
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

        Logger.ZLogInformation($"Launching game {gameId} with launch arguments: {launchArguments}");
        return true;
    }
}