namespace MuseDashModTools.Abstractions;

public interface IGameService
{
    Task LaunchModdedGameAsync();
    Task LaunchVanillaGameAsync();
}