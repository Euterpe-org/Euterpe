using System.Text.Json;
using System.Text.Json.Serialization;

namespace MuseDashModTools.Core.Converters;

public sealed class SemVersionConverter : JsonConverter<SemVersion>
{
    public override SemVersion? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var versionString = reader.GetString();
        return versionString is null ? null : SemVersion.Parse(versionString);
    }

    public override void Write(Utf8JsonWriter writer, SemVersion value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}