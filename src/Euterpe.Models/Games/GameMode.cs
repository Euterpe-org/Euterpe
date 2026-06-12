namespace Euterpe.Models.Games;

[JsonConverter(typeof(JsonStringEnumConverter<GameMode>))]
public enum GameMode
{
    Modded,
    Vanilla
}
