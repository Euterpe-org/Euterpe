namespace Euterpe.Abstractions;

public interface IGameService
{
    Task LaunchModdedGameAsync();
    Task LaunchVanillaGameAsync();
}