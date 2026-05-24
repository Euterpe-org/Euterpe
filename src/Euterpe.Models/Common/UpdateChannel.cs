namespace Euterpe.Models.Common;

[JsonConverter(typeof(JsonStringEnumConverter<UpdateChannel>))]
public enum UpdateChannel
{
    Stable,
    Prerelease
}