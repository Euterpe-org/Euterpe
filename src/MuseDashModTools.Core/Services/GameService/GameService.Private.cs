namespace MuseDashModTools.Core;

internal sealed partial class GameService
{
    private void LaunchGame(string launchArguments)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "steam://rungameid/774171" + launchArguments,
            UseShellExecute = true
        });

        Logger.ZLogInformation($"Launching game with launch arguments: {launchArguments}");
    }
}