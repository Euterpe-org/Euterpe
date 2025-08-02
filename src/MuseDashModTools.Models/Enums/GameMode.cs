namespace MuseDashModTools.Models.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<GameMode>))]
public enum GameMode
{
    Modded,
    Vanilla
}