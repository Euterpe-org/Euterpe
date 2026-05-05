namespace Euterpe.Models.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<GameId>))]
public enum GameId
{
    MuseDash = 1,
    MuseDash2 = 2
}