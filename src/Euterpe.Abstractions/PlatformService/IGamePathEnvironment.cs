namespace Euterpe.Abstractions;

public interface IGamePathEnvironment
{
    /// <summary>
    ///     Check whether the env variable pointing to the game folder is set correctly.
    /// </summary>
    bool IsSet();

    /// <summary>
    ///     Set the env variable pointing to the game folder.
    /// </summary>
    bool Set();
}
