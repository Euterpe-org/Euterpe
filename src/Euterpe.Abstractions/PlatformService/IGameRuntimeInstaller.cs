namespace Euterpe.Abstractions;

public interface IGameRuntimeInstaller
{
    const string DotnetRuntimeUrl = "https://aka.ms/dotnet/6.0/dotnet-runtime-win-x64.zip";

    /// <summary>
    ///     Check the runtime required by the game is installed.
    /// </summary>
    Task<bool> CheckInstalledAsync();

    /// <summary>
    ///     Install the runtime required by the game. Throws on failure.
    /// </summary>
    Task InstallAsync();
}
