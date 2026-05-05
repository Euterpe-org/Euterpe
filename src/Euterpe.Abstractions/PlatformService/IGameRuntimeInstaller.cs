namespace Euterpe.Abstractions;

public interface IGameRuntimeInstaller
{
    /// <summary>
    ///     Check the runtime required by the game is installed.
    /// </summary>
    Task<bool> CheckInstalledAsync();

    /// <summary>
    ///     Install the runtime required by the game.
    /// </summary>
    Task<bool> InstallAsync();
}