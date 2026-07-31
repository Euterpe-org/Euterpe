namespace Euterpe.Models.Common;

[EnumExtensions]
[JsonConverter(typeof(JsonStringEnumConverter<UpdateChannel>))]
public enum UpdateChannel
{
    Stable,
    Beta
}
