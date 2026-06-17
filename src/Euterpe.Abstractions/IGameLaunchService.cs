namespace Euterpe.Abstractions;

public interface IGameLaunchService
{
    Task LaunchModdedGameAsync();
    Task LaunchVanillaGameAsync();
}
