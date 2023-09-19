namespace MuseDashModToolsUI.Contracts;

public interface ILogAnalyzeService
{
    /// <summary>
    ///     Analyze log file
    /// </summary>
    Task AnalyzeLog();

    /// <summary>
    ///     Check whether the game is pirated
    /// </summary>
    /// <returns>Is pirate</returns>
    Task<bool> CheckPirate();

    /// <summary>
    ///     Check if MelonLoader version is correct
    /// </summary>
    /// <returns>Is correct version</returns>
    Task<bool> CheckMelonLoaderVersion();

    /// <summary>
    ///     If there's no log file, show no log file text
    ///     If there's log file, load it
    /// </summary>
    /// <returns>Log content</returns>
    Task<string> LoadLog();
}