namespace Euterpe.Models.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<AskType>))]
public enum AskType
{
    Always,
    YesAndNoAsk,
    NoAndNoAsk
}