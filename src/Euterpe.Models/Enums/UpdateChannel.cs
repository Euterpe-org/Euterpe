namespace Euterpe.Models.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<UpdateChannel>))]
public enum UpdateChannel
{
    Stable,
    Prerelease
}