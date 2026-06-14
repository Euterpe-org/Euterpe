namespace Euterpe.Models.Games;

[EnumExtensions]
[JsonConverter(typeof(JsonStringEnumConverter<GameMode>))]
public enum GameMode
{
    Modded,
    Vanilla
}
